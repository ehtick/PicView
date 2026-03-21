using Avalonia.Threading;
using PicView.Avalonia.StartUp;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.Navigation;

public static class NavigationSubscriptions
{
    public static void ModelSubscription(TabViewModel tabViewModel, MainWindowViewModel mainWindowViewModel)
    {
        // Subscribing with AvaloniaRenderingFrameProvider is faster and fixes not being able to navigate while gallery is loading
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.FileInfo, UIHelper2.GetFrameProvider)
                    .Subscribe(file =>
                    {
                        if (tabViewModel.Model.Image is null || tabViewModel.Model.PixelHeight is 0 || tabViewModel.Model.PixelWidth is 0)
                        {
                            return;
                        }

                        // Update title to reflect file changes
                        if (file is null || file.Length is 0)
                        {
                            var noImage = TranslationManager.Translation?.NoImage;
                            if (string.IsNullOrEmpty(noImage))
                            {
                                return;
                            }

                            tabViewModel.TabTitle.Value = noImage;
                            tabViewModel.TabTooltip.Value = noImage;
                            return;
                        }

                        // Trigger file changes to UI
                        tabViewModel.FileInfo.Value = file;
                        
                        tabViewModel.UpdateTabTitle();
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
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.Image, UIHelper2.GetFrameProvider)
                    .Subscribe(image =>
                    {
                        // Trigger image change to UI
                        tabViewModel.Image.Value = image;

                        if (Settings.WindowProperties.AutoFit)
                        {
                            WindowResizing2.SetSize(tabViewModel.Model.PixelWidth,
                                                    tabViewModel.Model.PixelHeight,
                                                    WindowResizeReason.Application,
                                                    mainWindowViewModel);
                        }

                        // Update tiff title if appropriate (there are no file changes in this instance
                        if (tabViewModel.Model.TiffNavigation is null)
                        {
                            return;
                        }

                        tabViewModel.UpdateTabTitle();
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

                Observable.EveryValueChanged(tabViewModel, tab => tab.Gallery.GalleryMode.Value, UIHelper2.GetFrameProvider)
                    .Skip(1)
                    .SubscribeAwait(async (mode, _) =>
                    {
                        if (mode is GalleryMode2.Docked or GalleryMode2.Expanded)
                        {
                            if (tabViewModel.Gallery.LoadingState is GalleryLoadingState.NotLoaded)
                            {
                                if (Application.Current.DataContext is not CoreViewModel core)
                                {
                                    return;
                                }
                                await GalleryLoader.LoadGalleryAsync(tabViewModel,
                                        tabViewModel.ImageIterator.Files,
                                        new AvaloniaThumbnailLoader(),
                                        core.SharedThumbnailCache,
                                        tabViewModel.GetTabCancellation().Token)
                                    .ConfigureAwait(false);
                            }
                        }
                    }, result =>
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
                        if (tabViewModel.ImageIterator is null)
                        {
                            return;
                        }
                        if (tabViewModel.Gallery.IsGalleryExpanded.Value)
                        {
                            tabViewModel.Gallery.ToggleGalleryCommand.Execute(Unit.Default);
                        }

                        await tabViewModel.ImageIterator.SkipToIndexAsync(index, tabViewModel.GetTabCancellation()).ConfigureAwait(false);
                    }, result =>
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