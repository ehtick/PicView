using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PicView.Core.Localization;

[JsonSourceGenerationOptions(AllowTrailingCommas = true)]
[JsonSerializable(typeof(LanguageModel))]
internal partial class LanguageSourceGenerationContext : JsonSerializerContext;

/// <summary>
/// Helper class for managing language-related tasks, including loading and switching languages.
/// </summary>
public static class TranslationManager
{
    /// <summary>
    /// The current language model containing translations.
    /// </summary>
    public static LanguageModel? Translation { get; private set; }

    /// <summary>
    /// Initializes the language model by setting all strings to empty.
    /// </summary>
    /// <remarks>
    /// This is used to defer loading translations until explicitly needed.
    /// </remarks>
    public static void Init()
    {
        Translation = new LanguageModel();
    }

    /// <summary>
    /// Retrieves the translated string for the given key.
    /// </summary>
    /// <param name="key">The key representing the translation string.</param>
    /// <returns>The translated string, or the key itself if no translation is found.</returns>
    public static string GetTranslation(string key)
    {
        if (Translation == null)
        {
            return key;
        }

        var propertyInfo = typeof(LanguageModel).GetProperty(key);
        return propertyInfo?.GetValue(Translation) as string ?? key;
    }

    /// <summary>
    /// Loads the language file based on the provided ISO language code.
    /// </summary>
    /// <param name="isoLanguageCode">The ISO language code (e.g., 'en', 'da').</param>
    /// <returns>Returns true if the language file was successfully loaded; false if an error occurred.</returns>
    public static async Task<bool> LoadLanguage(string isoLanguageCode)
    {
        var jsonLanguageFile = DetermineLanguageFilePath(isoLanguageCode);

        try
        {
            await LoadLanguageFromFileAsync(jsonLanguageFile).ConfigureAwait(false);
            return true;
        }
        catch (FileNotFoundException fnfEx)
        {
#if DEBUG
            Trace.WriteLine($"Language file not found: {fnfEx.Message}");
#endif
            return false;
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine($"{nameof(LoadLanguage)} exception:\n{ex.Message}");
#endif
            return false;
        }
    }

    /// <summary>
    /// Determines the correct language based on the system's current culture and loads the corresponding language file.
    /// </summary>
    public static async Task DetermineAndLoadLanguage()
    {
        var isoLanguageCode = DetermineCorrectLanguage();
        Settings.UIProperties.UserLanguage = isoLanguageCode;
        await LoadLanguage(isoLanguageCode).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a list of available language files in the language directory.
    /// </summary>
    /// <returns>An enumerable collection of paths to available language JSON files.</returns>
    public static IEnumerable<string> GetLanguages()
    {
        var languagesDirectory = GetLanguagesDirectory();
        return Directory.EnumerateFiles(languagesDirectory, "*.json", SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Determines the file path for the specified ISO language code.
    /// </summary>
    /// <param name="isoLanguageCode">The ISO language code representing the desired language.</param>
    /// <returns>The file path of the matching language file, or the English language file as a fallback.</returns>
    private static string DetermineLanguageFilePath(string isoLanguageCode)
    {
        var languagesDirectory = GetLanguagesDirectory();
        var matchingFiles = Directory.GetFiles(languagesDirectory, "*.json")
            .Where(file => Path.GetFileNameWithoutExtension(file)?.Equals(isoLanguageCode, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        return matchingFiles.FirstOrDefault() ?? Path.Combine(languagesDirectory, "en.json");
    }

    /// <summary>
    /// Loads a language from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the language JSON file.</param>
    /// <returns>A task that completes once the language is loaded.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the language file is not found.</exception>
    private static async Task LoadLanguageFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Language file not found: {filePath}");
        }

        var jsonString = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        var language = JsonSerializer.Deserialize(jsonString, typeof(LanguageModel), LanguageSourceGenerationContext.Default) as LanguageModel;
        Translation = language;
    }

    /// <summary>
    /// Retrieves the directory path where language files are stored.
    /// </summary>
    /// <returns>The path to the language files directory.</returns>
    private static string GetLanguagesDirectory()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/Languages/");
    }

    /// <summary>
    /// Determines the correct language code based on the system's current UI culture.
    /// </summary>
    /// <returns>The ISO language code to use for translations.</returns>
    public static string DetermineCorrectLanguage()
    {
        var userCulture = CultureInfo.CurrentUICulture;
        var baseLanguageCode = userCulture.TwoLetterISOLanguageName; // Gets 'da' from 'da-DK'

        // Handle special cases, e.g., Chinese or different regions.
        switch (baseLanguageCode)
        {
            case "zh":
                // Simplified Chinese vs Traditional Chinese
                return userCulture.Name switch
                {
                    "zh-TW" => "zh-TW",  // Traditional Chinese
                    "zh-HK" => "zh-TW",  // Treat Hong Kong as Traditional Chinese
                    _ => "zh-CN"         // Default to Simplified Chinese
                };
            case "de":
                // Handle German-speaking regions (Austria, Germany, Switzerland)
                return "de";  // Map all 'de-*' to 'de'
            default:
                // Fall back to the base language if it's available in the translation files
                return GetLanguages()
                    .Any(lang => Path.GetFileNameWithoutExtension(lang) == baseLanguageCode)
                    ? baseLanguageCode
                    : "en"; // Default to English if not found
        }
    }
}
