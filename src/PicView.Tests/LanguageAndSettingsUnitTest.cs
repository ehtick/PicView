using System.Text.Json;
using PicView.Core.Localization;
using PicView.Tests.LanguageTests;

namespace PicView.Tests;

public class LanguageAndSettingsUnitTest
{
    [Fact]
    public async Task CheckIfSettingsWorks()
    {
        await LoadSettingsAsync();
        Assert.NotNull(Settings);
        var testSave = await SaveSettingsAsync();
        Assert.True(testSave);
    }

    [Fact]
    public async Task CheckLanguages()
    {
        // Load the keys from the en.json file
        var enJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/Languages/en.json");
        var enKeys = await GetJsonKeys(enJsonPath);
    
        var languages = TranslationManager.GetLanguages();
        Assert.NotNull(languages);
    
        // Check each language file against en.json keys
        foreach (var languagePath in languages)
        {
            if (languagePath.Equals(enJsonPath, StringComparison.OrdinalIgnoreCase))
            {
                continue; // Skip the en.json file itself
            }
        
            var languageKeys = await GetJsonKeys(languagePath);
            var missingKeys = enKeys.Except(languageKeys).ToList();
            var extraKeys = languageKeys.Except(enKeys).ToList();
        
            Assert.False(missingKeys.Count != 0, $"Missing keys in {Path.GetFileName(languagePath)}: {string.Join(", ", missingKeys)}");
            Assert.True(extraKeys.Count == 0, $"Extra keys in {Path.GetFileName(languagePath)}: {string.Join(", ", extraKeys)}");
            Assert.True(enKeys.SetEquals(languageKeys), $"Key mismatch in {Path.GetFileName(languagePath)}");
        }
    
        await CheckDanishLanguage();
        await CheckDutchLanguage();
        await CheckEnglishLanguage();
        await CheckGermanLanguage();
        await CheckFrenchLanguage();
        await CheckItalianLanguage();
        await CheckKoreanLanguage();
        await CheckPolishLanguage();
        await CheckBrazilianPortugueseLanguage();
        await CheckRomanianLanguage();
        await CheckRussianLanguage();
        await CheckSpanishLanguage();
        await CheckSwedishLanguage();
        await CheckTurkishLanguage();
        await CheckChineseSimplifiedLanguage();
        await CheckChineseTraditionalLanguage();
    }

    private async Task<HashSet<string>> GetJsonKeys(string filePath)
    {
        var jsonString = await File.ReadAllTextAsync(filePath);
        var jsonDocument = JsonDocument.Parse(jsonString);
        var root = jsonDocument.RootElement;
    
        var keys = new HashSet<string>();
        foreach (var property in root.EnumerateObject())
        {
            keys.Add(property.Name);
        }
    
        return keys;
    }

    [Fact]
    public async Task ChangeLanguage()
    {
        await LoadSettingsAsync();
        Assert.NotNull(Settings);

        var exists = await TranslationManager.LoadLanguage("en");
        Assert.True(exists);
        Assert.Equal("Image", TranslationManager.Translation.Image);
        const Languages da = Languages.da;
        await TranslationManager.ChangeLanguage((int)da);
        Assert.Equal("Billede", TranslationManager.Translation.Image);
    }
    
    [Fact]
    public async Task CheckDanishLanguage()
    {
        await DanishUnitTest.CheckDanishLanguage();
    }
    
    [Fact]
    public async Task CheckDutchLanguage()
    {
        await DutchUnitTest.CheckDutchLanguage();
    }
    
    [Fact]
    public async Task CheckEnglishLanguage()
    {
        await EnglishUnitTest.CheckEnglishLanguage();
    }
    
    [Fact]
    public async Task CheckGermanLanguage()
    {
        await GermanUnitTest.CheckGermanLanguage();
    }
    
    [Fact]
    public async Task CheckFrenchLanguage()
    {
        await FrenchUnitTest.CheckFrenchLanguage();
    }

    [Fact]
    public async Task CheckItalianLanguage()
    {
        await ItalianUnitTest.CheckItalianLanguage();
    }

    [Fact]
    public async Task CheckKoreanLanguage()
    {
        await KoreanUnitTest.CheckKoreanLanguage();
    }

    [Fact]
    public async Task CheckPolishLanguage()
    {
        await PolishUnitTest.CheckPolishLanguage();
    }

    [Fact]
    public async Task CheckBrazilianPortugueseLanguage()
    {
        await BrazilianPortugueseUnitTest.CheckBrazilianPortugueseLanguage();
    }

    [Fact]
    public async Task CheckRomanianLanguage()
    {
        await RomanianUnitTest.CheckRomanianLanguage();
    }

    [Fact]
    public async Task CheckRussianLanguage()
    {
        await RussianUnitTest.CheckRussianLanguage();
    }
    
    [Fact]
    public async Task CheckSpanishLanguage()
    {
        await SpanishUnitTest.CheckSpanishLanguage();
    }

    [Fact]
    public async Task CheckSwedishLanguage()
    {
        await SwedishUnitTest.CheckSwedishLanguage();
    }

    [Fact]
    public async Task CheckTurkishLanguage()
    {
        await TurkishUnitTest.CheckTurkishLanguage();
    }

    [Fact]
    public async Task CheckChineseSimplifiedLanguage()
    {
        await ChineseSimplifiedUnitTest.CheckChineseSimplifiedLanguage();
    }

    [Fact]
    public async Task CheckChineseTraditionalLanguage()
    {
        await ChineseTraditionalUnitTest.CheckChineseTraditionalLanguage();
    }
}