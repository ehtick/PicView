using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using PicView.Core.Config;
using PicView.Core.Config.ConfigFileManagement;
using PicView.Core.DebugTools;

namespace PicView.Benchmarks.StartupBenchmarks;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsGenerationContext : JsonSerializerContext;

[MemoryDiagnoser] // track allocations
public class ConfigBenchmark
{
    private static AppSettings? Settings { get; set; }

    private static SettingsConfiguration? Configuration { get; set; }

    /// <summary>
    /// Global Configuration Support
    /// </summary>
    /// <remarks>
    /// Overrides any UserSettings with GlobalSettings if they are set
    /// </remarks>
    private static GlobalSettingsConfiguration? GlobalConfig { get; set; }

    private static AppSettings? GlobalSettings { get; set; }

    [Benchmark]
    public async ValueTask Initial()
    {
        await LoadSettingsAsync();
    }

    [Benchmark]
    public void WithStreamReader()
    {
        LoadSettingsWithStreamReader();
    }

    [Benchmark]
    public async ValueTask WithParallelTasks()
    {
        await LoadSettingsAsyncWithParallelTasks();
    }

    [Benchmark]
    public void ReadAllTextSync()
    {
        LoadSettingsAllTextSync();
    }

    [Benchmark]
    public void ReadAllLinesSync()
    {
        LoadSettingsLines();
    }

    [Benchmark]
    public void ReadAllBytesSync()
    {
        LoadSettingsBytes();
    }

    [Benchmark]
    public async ValueTask ReadAllBytesAsync()
    {
        await LoadSettingsBytesAsync();
    }

    [Benchmark]
    public async ValueTask ReadAllLinesAsync()
    {
        await LoadSettingsLinesAsync();
    }

    public static async ValueTask<bool> LoadSettingsAsync()
    {
        try
        {
            // Load global config (read-only, Program Path)
            GlobalConfig ??= new GlobalSettingsConfiguration();
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                await using var globalStream = File.OpenRead(GlobalConfig.LocalConfigPath);
                if (globalStream.Length > 0)
                {
                    GlobalSettings = await JsonSerializer.DeserializeAsync<AppSettings>(
                        globalStream, SettingsGenerationContext.Default.AppSettings).ConfigureAwait(false);
                }
            }

            // Load user config (User Profile or Program Path)
            Configuration ??= new SettingsConfiguration();
            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            if (File.Exists(userPath))
            {
                await using var userStream = File.OpenRead(userPath);
                if (userStream.Length > 0)
                {
                    Settings = await JsonSerializer.DeserializeAsync<AppSettings>(
                        userStream, SettingsGenerationContext.Default.AppSettings).ConfigureAwait(false);
                }
            }

            // Fallback to defaults if no user config found
            // Fallback to defaults if no user config found
            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            // Apply Global Overrides
            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    public static bool LoadSettingsWithStreamReader()
    {
        try
        {
            // Load global config (read-only, Program Path)
            GlobalConfig ??= new GlobalSettingsConfiguration();
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                using var streamReader = new StreamReader(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    streamReader.ReadToEnd(), SettingsGenerationContext.Default.AppSettings);
            }

            // Load user config (User Profile or Program Path)
            Configuration ??= new SettingsConfiguration();
            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            if (File.Exists(userPath))
            {
                using var streamReader = new StreamReader(userPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    streamReader.ReadToEnd(), SettingsGenerationContext.Default.AppSettings);
            }

            // Fallback to defaults if no user config found
            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            // Apply Global Overrides
            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }


    public static async ValueTask<bool> LoadSettingsAsyncWithParallelTasks()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            // Parallel loading of both configs
            var globalTask = File.Exists(GlobalConfig.LocalConfigPath)
                ? LoadConfigAsync(GlobalConfig.LocalConfigPath)
                : ValueTask.FromResult<AppSettings?>(null);

            var userTask = File.Exists(userPath)
                ? LoadConfigAsync(userPath)
                : ValueTask.FromResult<AppSettings?>(null);

            GlobalSettings = await globalTask.ConfigureAwait(false);
            Settings = await userTask.ConfigureAwait(false);

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    private static async ValueTask<AppSettings?> LoadConfigAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        if (stream.Length == 0)
        {
            return null;
        }

        return await JsonSerializer.DeserializeAsync<AppSettings>(
            stream, SettingsGenerationContext.Default.AppSettings).ConfigureAwait(false);
    }

    public static bool LoadSettingsAllTextSync()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            // Synchronous loading - fastest for startup
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                var json = File.ReadAllText(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (File.Exists(userPath))
            {
                var json = File.ReadAllText(userPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    public static bool LoadSettingsLines()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            // Synchronous loading - fastest for startup
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                var json = File.ReadAllText(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (File.Exists(userPath))
            {
                var json = File.ReadAllText(userPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    public static bool LoadSettingsBytes()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            // Synchronous loading - fastest for startup
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                var bytes = File.ReadAllBytes(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    bytes, SettingsGenerationContext.Default.AppSettings);
            }

            if (File.Exists(userPath))
            {
                var bytes = File.ReadAllBytes(GlobalConfig.LocalConfigPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    bytes, SettingsGenerationContext.Default.AppSettings);
            }

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    public static async ValueTask<bool> LoadSettingsBytesAsync()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            // Synchronous loading - fastest for startup
            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                var bytes = await File.ReadAllBytesAsync(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    bytes, SettingsGenerationContext.Default.AppSettings);
            }

            if (File.Exists(userPath))
            {
                var bytes = await File.ReadAllBytesAsync(GlobalConfig.LocalConfigPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    bytes, SettingsGenerationContext.Default.AppSettings);
            }

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    public static async ValueTask<bool> LoadSettingsLinesAsync()
    {
        try
        {
            GlobalConfig ??= new GlobalSettingsConfiguration();
            Configuration ??= new SettingsConfiguration();

            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            if (File.Exists(GlobalConfig.LocalConfigPath))
            {
                var json = await File.ReadAllTextAsync(GlobalConfig.LocalConfigPath);
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (File.Exists(userPath))
            {
                var json = await File.ReadAllTextAsync(userPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(
                    json, SettingsGenerationContext.Default.AppSettings);
            }

            if (Settings is null)
            {
                Settings = GetDefaults();
                return false;
            }

            if (GlobalSettings != null)
            {
                //ApplyOverrides(Settings, GlobalSettings);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }
}

/*

// * Summary *
                                                                                                                                                                                                                                                             
BenchmarkDotNet v0.15.5, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores                                                                                                                                                                                          
.NET SDK 10.0.100-rc.2.25502.107                                                                                                                                                                                                                             
  [Host]     : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4                                                                                                                                                                      
  DefaultJob : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4                                                                                                                                                                      
                                                                                                                                                                                                                                                             

| Method            | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------ |----------:|---------:|---------:|-------:|----------:|
| Initial           | 126.22 us | 2.219 us | 2.374 us |      - |   3.97 KB |                                                                                                                                                                                 
| WithStreamReader  |  47.85 us | 0.195 us | 0.152 us | 0.3662 |  23.16 KB |
| WithParallelTasks | 125.60 us | 1.810 us | 1.693 us |      - |   4.12 KB |
| ReadAllTextSync   |  47.63 us | 0.269 us | 0.225 us | 0.3662 |  23.16 KB |
| ReadAllLinesSync  |  48.07 us | 0.516 us | 0.482 us | 0.3662 |  23.16 KB |
| ReadAllBytesSync  |  31.35 us | 0.058 us | 0.045 us |      - |   2.89 KB |
| ReadAllBytesAsync |  31.61 us | 0.049 us | 0.044 us |      - |   2.89 KB |
| ReadAllLinesAsync | 180.67 us | 2.893 us | 2.706 us | 0.4883 |  21.34 KB |

*/