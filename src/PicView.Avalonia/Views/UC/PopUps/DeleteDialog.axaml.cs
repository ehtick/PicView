using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.Localization;

namespace PicView.Avalonia.Views.UC.PopUps;

public partial class DeleteDialog : AnimatedPopUp
{
    public DeleteDialog(string prompt, string file, bool recycle)
    {
        InitializeComponent();
        ConfirmButtonText.Text = recycle ?
            TranslationManager.Translation.MoveToRecycleBin :
            TranslationManager.Translation.DeleteFile;
        
        Loaded += delegate
        {
            PromptText.Text = prompt;
            PromptFileName.Text = Path.GetFileName(file) + "?";
            CancelButton.Click += async delegate { await AnimatedClosing(); };
            ConfirmButton.Click += async delegate
            {
                if (DataContext is not MainViewModel vm)
                {
                    return;
                }
                var tasks = new List<Task>();
                var success = vm.PlatformService.DeleteFile(file, true);
                tasks.Add(success);
                var animatedClosing = AnimatedClosing();
                tasks.Add(animatedClosing);
                await Task.WhenAll(tasks);
                if (!success.Result && File.Exists(file))
                {
                    TooltipHelper.ShowTooltipMessage(TranslationManager.Translation.UnexpectedError);
                }
            };

            Focus();

            KeyDown += (_, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        ConfirmButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        break;
                    case Key.Escape:
                        CancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        break;
                }

                e.Handled = true;
            };
        };
    }
}
