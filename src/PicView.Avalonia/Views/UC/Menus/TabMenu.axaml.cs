using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class TabMenu : AnimatedMenu
{
    public TabMenu()
    {
        if (UIHelper.GetMainView.DataContext is not MainViewModel vm)
        {
            return;
        }
        vm.TabMenu ??= new TabMenuViewModel();
        InitializeComponent();
        Observable.EveryValueChanged(vm.TabMenu, x => x.MenuCarouselIndex.CurrentValue, UIHelper.GetFrameProvider)
            .Subscribe(i =>
            {
                MainCarousel.SelectedIndex = i;
            });
    }
}