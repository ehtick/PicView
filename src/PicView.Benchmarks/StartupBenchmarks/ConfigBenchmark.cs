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
    public async ValueTask OpenReadAsync()
    {
        await LoadSettingsAsync();
    }

    [Benchmark]
    public void ReadToEnd()
    {
        LoadSettingsWithStreamReader();
    }

    [Benchmark]
    public async ValueTask OpenReadAsyncOptimized()
    {
        await LoadSettingsAsyncOptimized();
    }

    [Benchmark]
    public void ReadAllTextSync()
    {
        LoadSettingsSync();
    }

    [Benchmark]
    public void ReadAllLinesSync()
    {
        LoadSettingsLines();
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
            Settings ??= GetDefaults();

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
                streamReader.ReadToEnd();
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    streamReader.BaseStream, SettingsGenerationContext.Default.AppSettings);
            }

            // Load user config (User Profile or Program Path)
            Configuration ??= new SettingsConfiguration();
            var userPath = ConfigFileManager.ResolveDefaultConfigPath(Configuration);
            Configuration.CorrectPath = userPath;

            if (File.Exists(userPath))
            {
                using var streamReader = new StreamReader(userPath);
                streamReader.ReadToEnd();
                GlobalSettings = JsonSerializer.Deserialize<AppSettings>(
                    streamReader.BaseStream, SettingsGenerationContext.Default.AppSettings);
            }

            // Fallback to defaults if no user config found
            Settings ??= GetDefaults();

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


    public static async ValueTask<bool> LoadSettingsAsyncOptimized()
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

            Settings ??= GetDefaults();

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

    public static bool LoadSettingsSync()
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

            Settings ??= GetDefaults();

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

            Settings ??= GetDefaults();

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

            // Synchronous loading - fastest for startup
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

            Settings ??= GetDefaults();

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

* Summary *

BenchmarkDotNet v0.15.2, Windows 10 (10.0.19045.6216/22H2/2022Update)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100-preview.7.25380.108
  [Host]     : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


| Method                 | Mean     | Error   | StdDev  | Gen0   | Allocated |
|----------------------- |---------:|--------:|--------:|-------:|----------:|
| OpenReadAsync          | 229.4 us | 3.93 us | 3.28 us |      - |   3.88 KB |                                                                                                                                                                               
| ReadToEnd              | 177.2 us | 3.43 us | 3.67 us | 0.4883 |  24.66 KB |
| OpenReadAsyncOptimized | 233.8 us | 4.22 us | 3.95 us |      - |   4.04 KB |
| ReadAllTextSync        | 167.3 us | 2.39 us | 1.99 us | 0.2441 |  23.01 KB |
| ReadAllLinesSync       | 166.6 us | 1.16 us | 1.03 us | 0.2441 |  23.01 KB |
| ReadAllLinesAsync      | 288.0 us | 4.83 us | 4.52 us | 0.4883 |   21.1 KB |

*/