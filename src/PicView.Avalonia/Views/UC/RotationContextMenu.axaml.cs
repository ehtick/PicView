using Avalonia.Controls;
using Avalonia.Interactivity;
using PicView.Avalonia.ImageTransformations;
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
        _disposable = Observable.EveryValueChanged(this, x => IsOpen, UIHelper2.GetFrameProvider)
            .Subscribe(_ => { UpdateRotation(); },
                DebugHelper.LogError(nameof(RotationContextMenu), nameof(UpdateSubscription)));

        Rotation0Item.Click += Rotation0ItemOnClick;
        Rotation90Item.Click += Rotation90ItemOnClick;
        Rotation180Item.Click += Rotation180ItemOnClick;
        Rotation270Item.Click += Rotation270ItemOnClick;
    }

    private void Rotation270ItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        RotationManager.Rotate(vm, 270);
    }

    private void Rotation180ItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        RotationManager.Rotate(vm, 180);
    }

    private void Rotation90ItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        RotationManager.Rotate(vm, 90);
    }

    private void Rotation0ItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        RotationManager.Rotate(vm, 0);
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
        Rotation0Item.Click -= Rotation0ItemOnClick;
        Rotation90Item.Click -= Rotation90ItemOnClick;
        Rotation180Item.Click -= Rotation180ItemOnClick;
        Rotation270Item.Click -= Rotation270ItemOnClick;
    }
}