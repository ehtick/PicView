using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.Localization;

namespace PicView.Avalonia.Views.UC;

public partial class EditableTitlebar : UserControl
{
    public EditableTitlebar()
    {
        InitializeComponent();
        LostFocus += HandleLostFocus;
        PointerEntered += HandlePointerEntered;
        PointerPressed += HandlePointerPressed;
        TextBox.LostFocus += HandleLostFocus;
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!UIHelper.TryGetMainViewModel(out var vm) ||
            !e.GetCurrentPoint(this).Properties.IsRightButtonPressed ||
            vm.IsEditableTitlebarOpen)
        {
            return;
        }

        vm.IsEditableTitlebarOpen = true;
        SelectFileName();
    }

    private void HandlePointerEntered(object? sender, PointerEventArgs e)
    {
        if (!UIHelper.TryGetMainViewModel(out var vm))
        {
            return;
        }

        Cursor = vm.IsEditableTitlebarOpen
            ? new Cursor(StandardCursorType.Ibeam)
            : new Cursor(StandardCursorType.Arrow);
    }

    private void HandleLostFocus(object? sender, RoutedEventArgs e) => CloseTitlebar();

    public void CloseTitlebar()
    {
        TextBox.ClearSelection();
        if (!UIHelper.TryGetMainViewModel(out var vm))
        {
            return;
        }

        vm.IsEditableTitlebarOpen = false;
        Cursor = new Cursor(StandardCursorType.Arrow);
        MainKeyboardShortcuts.IsKeysEnabled = true;
        TextBlock.Text = vm.PicViewer.Title.CurrentValue;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!UIHelper.TryGetMainViewModel(out var vm))
        {
            return;
        }

        if (!vm.IsEditableTitlebarOpen)
        {
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            CloseTitlebar();
            e.Handled = true;
            TopLevel.GetTopLevel(this)?.Focus();
            return;
        }

        MainKeyboardShortcuts.IsKeysEnabled = false;
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!UIHelper.TryGetMainViewModel(out var vm))
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

        if (e.Key == Key.Enter)
        {
            var oldPath = vm.PicViewer.FileInfo.CurrentValue.FullName;
            var newPath = Path.Combine(vm.PicViewer.FileInfo.CurrentValue.DirectoryName, TextBox.Text);
            Task.Run(async () =>
            {
                if (newPath == oldPath)
                {
                    await ShowFileExistsErrorAsync(vm);
                    return;
                }
                var isFileRenamed = await FileRenamer.AttemptRenameAsync(oldPath, newPath, vm);
                MainKeyboardShortcuts.IsKeysEnabled = true;
                if (isFileRenamed)
                {
                    vm.IsLoading = false;
                    vm.IsEditableTitlebarOpen = false;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        TextBox.ClearSelection();
                        Cursor = new Cursor(StandardCursorType.Arrow);
                        UIHelper.GetMainView.Focus();
                    });
                }
            });
        }
        else if (e.Key == Key.Escape)
        {
            UIHelper.GetMainView.Focus();
            MainKeyboardShortcuts.IsKeysEnabled = true;
        }
    }
    
    private async Task ShowFileExistsErrorAsync(MainViewModel vm)
    {
        CloseTitlebar();
        vm.IsLoading = false;
        await TooltipHelper.ShowTooltipMessageAsync(TranslationManager.GetTranslation("FileAlreadyExistsError"), true);
    }

    public void SelectFileName()
    {
        if (!UIHelper.TryGetMainViewModel(out var vm) || vm.PicViewer.FileInfo is null)
        {
            return;
        }

        var filename = vm.PicViewer.FileInfo.CurrentValue.Name;
        TextBox.Text = filename;

        var start = TextBox.Text.Length - filename.Length;
        var end = Path.GetFileNameWithoutExtension(filename).Length;
        TextBox.SelectionStart = start;
        TextBox.SelectionEnd = end;

        vm.IsEditableTitlebarOpen = true;
        Cursor = new Cursor(StandardCursorType.Ibeam);
        TextBox.Focus();
    }
}