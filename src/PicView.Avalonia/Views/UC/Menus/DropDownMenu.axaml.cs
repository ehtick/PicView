using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ViewModels;
using R3;

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
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        Observable.EveryValueChanged(vm.MainWindow.TopTitlebarViewModel.DropDownMenu, x => x.MenuCarouselIndex.CurrentValue, UIHelper.GetFrameProvider)
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
}