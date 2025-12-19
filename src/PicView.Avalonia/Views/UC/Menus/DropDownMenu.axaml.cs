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
        if (UIHelper.GetMainView.DataContext is not MainViewModel vm)
        {
            return;
        }
        vm.DropDownMenu ??= new DropDownMenuViewModel();
        InitializeComponent();
        Observable.EveryValueChanged(vm.DropDownMenu, x => x.MenuCarouselIndex.CurrentValue, UIHelper.GetFrameProvider)
            .Subscribe(i =>
            {
                MainCarousel.SelectedIndex = i;
            });
    }
}