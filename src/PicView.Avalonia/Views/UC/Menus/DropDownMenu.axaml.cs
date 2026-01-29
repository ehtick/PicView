using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using R3;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class DropDownMenu : AnimatedMenu
{
    public DropDownMenu()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        Observable.EveryValueChanged(vm.TopTitlebarViewModel.DropDownMenu, x => x.MenuCarouselIndex.CurrentValue, UIHelper2.GetFrameProvider)
            .Subscribe(i =>
            {
                MainCarousel.SelectedIndex = i;
            });
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        // Trigger closing animation
        IsOpen = false;
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        // Let view model know it is closed
        vm.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
    }
}