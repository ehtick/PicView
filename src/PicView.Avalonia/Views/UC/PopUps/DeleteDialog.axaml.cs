using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.CustomControls;
using PicView.Core.FileHandling;
using PicView.Core.Localization;

namespace PicView.Avalonia.Views.UC.PopUps;

public partial class DeleteDialog : AnimatedPopUp
{
    public DeleteDialog(string prompt, string file, bool recycle)
    {
        InitializeComponent();
        if (recycle)
        {
            ConfirmButtonText.Text = TranslationManager.Translation.MoveToRecycleBin;
        }
        else
        {
            ConfirmButtonText.Text = TranslationManager.Translation.DeleteFile;
        }
        Loaded += delegate
        {
            PromptText.Text = prompt;
            PromptFileName.Text = Path.GetFileName(file) + "?";
            CancelButton.Click += async delegate { await AnimatedClosing(); };
            ConfirmButton.Click += async delegate
            {
                FileDeletionHelper.DeleteFileWithErrorMsg(file, recycle);
                await AnimatedClosing();
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
