using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicView.Avalonia.Input;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;

namespace PicView.Avalonia.Views.UC;

public partial class EditableTitlebar : UserControl
{
    public EditableTitlebar()
    {
        InitializeComponent();
        LostFocus += OnLostFocus;
        PointerEntered += OnPointerEntered;
        PointerPressed += OnPointerPressed;
        TextBox.LostFocus += OnLostFocus;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        if (vm.IsEditableTitlebarOpen)
        {
            return;
        }

        vm.IsEditableTitlebarOpen = true;
        SelectFileName();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        Cursor = vm.IsEditableTitlebarOpen ?
            new Cursor(StandardCursorType.Ibeam) :
            new Cursor(StandardCursorType.Arrow);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        CloseTitlebar();
    }
    
    public void CloseTitlebar()
    {
        TextBox.ClearSelection();
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        vm.IsEditableTitlebarOpen = false;
        Cursor = new Cursor(StandardCursorType.Arrow);
        MainKeyboardShortcuts.IsKeysEnabled = true;
        TextBlock.Text = vm.Title;
    }
    
    #region Rename
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!vm.IsEditableTitlebarOpen)
        {
            // Don't pass the key to the main window
            // Prevents keydown event being registered twice
            e.Handled = true;  
            return;
        }
        
        if (e.Key is Key.Escape)
        {
            CloseTitlebar();
            e.Handled = true;
            var topLevel = TopLevel.GetTopLevel(this);
            topLevel?.Focus();
            return;
        }
        MainKeyboardShortcuts.IsKeysEnabled = false;
        base.OnKeyDown(e);
    }
    
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!vm.IsEditableTitlebarOpen)
        {
            if (e.Key != Key.Escape)
            {
                _ = MainKeyboardShortcuts.MainWindow_KeysDownAsync(e).ConfigureAwait(false);
            }
            return;
        }
        
        if (e.Key is Key.Enter)
        {
            _ = HandleRename();
            MainKeyboardShortcuts.IsKeysEnabled = true;
        }
        else if (e.Key == Key.Escape)
        {
            UIHelper.GetMainView.Focus();
            MainKeyboardShortcuts.IsKeysEnabled = true;
        }
    }

    private async Task HandleRename()
    {
        // TODO Add to a separate helper class
        
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (vm.PicViewer.FileInfo is { Exists: false })
        {
            return;
        }
        
        vm.IsLoading = true;
        var oldPath = vm.PicViewer.FileInfo.FullName;
        var newPath = Path.Combine(vm.PicViewer.FileInfo.DirectoryName, TextBox.Text);

        if (File.Exists(newPath))
        {
            CloseTitlebar();
            vm.IsLoading = false;
            // Show error message to user
            // TODO Translate error message
            await TooltipHelper.ShowTooltipMessageAsync(TranslationManager.GetTranslation("FileAlreadyExistsError"), true);
            return;
        }
        
        // Handle renaming with different extensions
        if (Path.GetExtension(newPath) != Path.GetExtension(oldPath))
        {
            var saved = await SaveImageFileHelper.SaveImageAsync(stream: null, path: oldPath, destination: newPath, width:null, height: null, quality: null, ext: Path.GetExtension(newPath)).ConfigureAwait(false);
            while (FileHelper.IsFileInUse(oldPath))
            {
                await Task.Delay(50); // Fixes "this action can't be completed because the file is open"
            }

            if (saved)
            {
                var deleteMsg = FileDeletionHelper.DeleteFileWithErrorMsg(oldPath, false);
                if (!string.IsNullOrWhiteSpace(deleteMsg))
                {
                    // Show error message to user
                    await TooltipHelper.ShowTooltipMessageAsync(deleteMsg);
                    vm.IsLoading = false;
                    return;
                }
            }
            await End();
        }
        else
        {
            var renamed = FileHelper.RenameFile(oldPath, newPath);
            if (!renamed)
            {
                // TODO Show error message
                await TooltipHelper.ShowTooltipMessageAsync(TranslationManager.Translation.UnexpectedError);
                return;
            }
            await End();
        }

        return;

        async Task End()
        {
            vm.IsLoading = false;
            vm.IsEditableTitlebarOpen = false;
            MainKeyboardShortcuts.IsKeysEnabled = true;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TextBox.ClearSelection();
                Cursor = new Cursor(StandardCursorType.Arrow);
                UIHelper.GetMainView.Focus();
            });
            await NavigationManager.LoadPicFromFile(newPath, vm);
        }
    }
    
    public void SelectFileName()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        
        if (vm.PicViewer.FileInfo is null)
        {
            return;
        }
        
        TextBox.Text = vm.PicViewer.FileInfo.Name;
        var filename = vm.PicViewer.FileInfo.Name;
        var start = TextBox.Text.Length - filename.Length;
        var end = Path.GetFileNameWithoutExtension(filename).Length;
        TextBox.SelectionStart = start;
        TextBox.SelectionEnd = end;
        vm.IsEditableTitlebarOpen = true;
        Cursor = new Cursor(StandardCursorType.Ibeam);
        TextBox.Focus();
    }

    #endregion
}
