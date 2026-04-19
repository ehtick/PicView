using Avalonia.Controls;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC;

public partial class RotationContextMenu : ContextMenu, IDisposable
{
    protected override Type StyleKeyOverride => typeof(ContextMenu);
    private IDisposable? _disposable;
    
    public RotationContextMenu()
    {
        InitializeComponent();
    }

    public void UpdateSubscription()
    {
        _disposable = Observable.EveryValueChanged(this, x => IsOpen, UIHelper.GetFrameProvider)
            .Subscribe(_ => { UpdateRotation(); },
                DebugHelper.LogError(nameof(RotationContextMenu), nameof(UpdateSubscription)));
    }

    private void UpdateRotation()
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        Rotation0Item.IsChecked = false;
        Rotation90Item.IsChecked = false;
        Rotation180Item.IsChecked = false;
        Rotation270Item.IsChecked = false;
        switch (vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue)
        {
            case 0:
                Rotation0Item.IsChecked = true;
                break;
            case 90:
                Rotation90Item.IsChecked = true;
                break;
            case 180:
                Rotation180Item.IsChecked = true;
                break;
            case 270:
                Rotation270Item.IsChecked = true;
                break;
        }
    }

    public void Dispose()
    {
        _disposable?.Dispose();
        _disposable = null;
        
        GC.SuppressFinalize(this);
    }
}