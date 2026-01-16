using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.ViewModels;
using PicView.Core.Config;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using ZLinq;

namespace PicView.Avalonia.Services;

public class LanguageService : ILanguageService
{
    public async ValueTask UpdateLanguageAsync(string languageCode)
    {
        await TranslationManager.LoadLanguage(languageCode).ConfigureAwait(false);
        
        var vm = Application.Current?.DataContext;
        if (vm is MainViewModel mainVm)
        {
             await LanguageUpdater.UpdateLanguageAsync(mainVm.Translation, mainVm.PicViewer, true).ConfigureAwait(false);
        }
        else if (vm is CoreViewModel coreVm)
        {
             coreVm.Translation.UpdateLanguage();
        }
    }

    public IEnumerable<(string Code, string DisplayName)> GetAvailableLanguages()
    {
        return TranslationManager.GetLanguages().ToList()
                .Select(filePath =>
                {
                    var langCode = Path.GetFileNameWithoutExtension(filePath.Name);
                    var displayName = new System.Globalization.CultureInfo(langCode).DisplayName;
                    return (LanguageCode: langCode, DisplayName: displayName);
                })
                .OrderBy(x => x.DisplayName)
                .Select(x => (x.LanguageCode, x.DisplayName));
    }
}
