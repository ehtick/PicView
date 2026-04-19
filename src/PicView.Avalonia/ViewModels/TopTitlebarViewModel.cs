using PicView.Core.ViewModels;
using R3;

// TODO deprecated, delete
namespace PicView.Avalonia.ViewModels;

public class TopTitlebarViewModel
{
    public DropDownMenuViewModel DropDownMenu { get; } = new();
    
    public BindableReactiveProperty<bool> IsMainMenuVisible { get; } = new();
    public BindableReactiveProperty<bool> IsEditableTitlebarVisible { get; } = new(true);
    public BindableReactiveProperty<bool> IsGalleryButtonVisible { get; } = new(true);
    public BindableReactiveProperty<bool> IsMenuButtonVisible { get; } = new(true);
    public BindableReactiveProperty<bool> IsBtnPanelVisible { get; } = new(true);

    public ReactiveCommand? ToggleMenuCommand { get; private set; }
    public ReactiveCommand? OpenMenuCommand { get; private set; }
    
    public ReactiveCommand? ToggleDropDownMenuCommand { get; private set; }

    public TopTitlebarViewModel()
    {
        ToggleMenuCommand = new ReactiveCommand(ToggleMenu);
        OpenMenuCommand = new ReactiveCommand(OpenMenu);
        ToggleDropDownMenuCommand = new ReactiveCommand(ToggleDropDownMenu);
    }

    private void ToggleDropDownMenu(Unit unit)
    {
        DropDownMenu?.IsDropDownMenuVisible.Value = !DropDownMenu.IsDropDownMenuVisible.CurrentValue;
    }

    public void ToggleMenu(Unit unit)
        => ToggleMenu();

    public void ToggleMenu()
    {
        if (IsMainMenuVisible.CurrentValue)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu(Unit unit)
        => OpenMenu();

    public void OpenMenu()
    {
        IsMainMenuVisible.Value = true;
        IsEditableTitlebarVisible.Value = false;
        IsGalleryButtonVisible.Value = false;
        IsMenuButtonVisible.Value = false;

        // if (UIHelper.GetMainView.DataContext is not MainViewModel vm)
        // {
        //     return;
        // }
        //
        // CropFunctions.DetermineIfShouldBeEnabled(vm);
        // vm.PicViewer.ShouldOptimizeImageBeEnabled.Value =
        //     ConversionHelper.DetermineIfOptimizeImageShouldBeEnabled(vm.PicViewer.FileInfo?.CurrentValue);
    }

    public void CloseMenu(Unit unit)
        => CloseMenu();

    public void CloseMenu()
    {
        IsMainMenuVisible.Value = false;
        IsEditableTitlebarVisible.Value = true;
        IsGalleryButtonVisible.Value = true;
        IsMenuButtonVisible.Value = true;
    }
}