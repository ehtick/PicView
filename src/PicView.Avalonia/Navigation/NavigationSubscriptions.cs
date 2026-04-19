using Avalonia.Threading;
using PicView.Avalonia.StartUp;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using R3;

namespace PicView.Avalonia.Navigation;

public static class NavigationSubscriptions
{
    public static void ModelSubscription(TabViewModel tabViewModel, MainWindowViewModel mainWindowViewModel)
    {
        // Subscribing with AvaloniaRenderingFrameProvider is faster and fixes not being able to navigate while gallery is loading
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.CurrentValue.FileInfo, UIHelper.GetFrameProvider)
                    .Subscribe(file =>
                    {
                        UpdateImage2.UpdateFileInfo(tabViewModel, file);
                    }, static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.CurrentValue.Image, UIHelper.GetFrameProvider)
                    .Subscribe(_ =>
                    {
                        UpdateImage2.ChangeImage(tabViewModel, mainWindowViewModel);
                    }, static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);

                Observable.EveryValueChanged(tabViewModel, tab => tab.Gallery.GalleryMode.Value, UIHelper.GetFrameProvider)
                    .Skip(1)
                    .SubscribeAwait(async (mode, _) =>
                    {
                        await UpdateGallery.LoadGalleryIfDockedOrExpanded(tabViewModel, mode);
                    },  static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
                tabViewModel.Gallery.OpenSelectedItemCommand
                    .SubscribeAwait(async (index, _) =>
                    {
                        await UpdateGallery.ToggleGalleryAndLoadItem(tabViewModel, index);
                    }, static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
            });
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), e);
        }
    }
}