using Avalonia.Threading;
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
        Dispatcher.UIThread.Invoke(() =>
        {
            Observable.EveryValueChanged(tabViewModel, tab => tab.Model.FileInfo, UIHelper.GetFrameProvider)
                .Subscribe(file =>
                {
                    UpdateImage.UpdateFileInfo(tabViewModel, file);
                }, DebugHelper.LogError(nameof(NavigationSubscriptions), nameof(UpdateImage)))
                .AddTo(tabViewModel.Disposables);
            Observable.EveryValueChanged(tabViewModel, tab => tab.Model.Image, UIHelper.GetFrameProvider)
                .Subscribe(_ =>
                {
                    UpdateImage.ChangeImage(tabViewModel, mainWindowViewModel);
                }, DebugHelper.LogError(nameof(NavigationSubscriptions), nameof(UpdateImage)))
                .AddTo(tabViewModel.Disposables);

            Observable.EveryValueChanged(tabViewModel, tab => tab.Gallery.GalleryMode.Value, UIHelper.GetFrameProvider)
                .Skip(1)
                .SubscribeAwait(async (mode, _) =>
                {
                    await UpdateGallery.LoadGalleryIfDockedOrExpanded(tabViewModel, mode);
                }, DebugHelper.LogError(nameof(NavigationSubscriptions), nameof(UpdateGallery)))
                .AddTo(tabViewModel.Disposables);
            tabViewModel.Gallery.OpenSelectedItemCommand
                .SubscribeAwait(async (index, _) =>
                {
                    await UpdateGallery.ToggleGalleryAndLoadItem(tabViewModel, index);
                }, DebugHelper.LogError(nameof(NavigationSubscriptions), nameof(UpdateGallery)))
                .AddTo(tabViewModel.Disposables);
        });
    }
}