using System.Diagnostics;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GallerySharedSettingsViewModel
{
    private bool _isInitialized;
    
    public ReactiveCommand<GalleryDockPosition> SetDockPositionCommand { get; } = new();
    public ReactiveCommand<Unit> CloseGalleryCommand { get; } = new();

    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);
    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    public ReactiveCommand<string> SetStretchModeCommand { get; } = new();

    public BindableReactiveProperty<bool> IsTopDocked { get; } = new();
    public BindableReactiveProperty<bool> IsBottomDocked { get; } = new();
    public BindableReactiveProperty<bool> IsLeftDocked { get; } = new();
    public BindableReactiveProperty<bool> IsRightDocked { get; } = new();

    public BindableReactiveProperty<bool> IsDockedGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public BindableReactiveProperty<bool> IsGalleryDocked { get; } = new(Settings.Gallery.IsGalleryDocked);

    public BindableReactiveProperty<double> DockedGalleryItemSize { get; } =
        new(Settings.Gallery.BottomGalleryItemSize);

    public BindableReactiveProperty<double> DockedGalleryMaxItemSize { get; } =
        new(GalleryDefaults.MaxBottomGalleryItemHeight);

    public BindableReactiveProperty<double> DockedGalleryMinItemSize { get; } =
        new(GalleryDefaults.MinBottomGalleryItemHeight);

    public BindableReactiveProperty<double> ExpandedGalleryItemSize { get; } =
        new(Settings.Gallery.ExpandedGalleryItemSize);

    public BindableReactiveProperty<double> ExpandedGalleryMaxItemSize { get; } =
        new(GalleryDefaults.MaxFullGalleryItemHeight);

    public BindableReactiveProperty<double> ExpandedGalleryMinItemSize { get; } =
        new(GalleryDefaults.MinFullGalleryItemHeight);

    public BindableReactiveProperty<double> GalleryItemSpacing { get; } = new(Settings.Gallery.ItemSpacing);
    public BindableReactiveProperty<double> GalleryLineSpacing { get; } = new(Settings.Gallery.LineSpacing);

    public BindableReactiveProperty<int> DockedGalleryStretchIndex { get; } =
        new(GetStretchIndex(Settings.Gallery.BottomGalleryStretchMode));

    public BindableReactiveProperty<int> FullGalleryStretchIndex { get; } =
        new(GetStretchIndex(Settings.Gallery.FullGalleryStretchMode));

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
#if DEBUG
        Debug.Assert(Settings?.Gallery is not null);
#endif
        SetStretchModeCommand.Subscribe(mode => { GalleryStretch.Value = mode; }, result =>
        {
#if DEBUG
            if (result is { IsFailure: true, Exception: not null })
            {
                DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize), result.Exception);
            }
#endif
        });

        UpdateDockPositionProperties();
        ToggleGalleryVisibilitySubscription();

        Observable.EveryValueChanged(Settings.Gallery, x => x.DockPosition)
            .Subscribe(_ => { UpdateDockPositionProperties(); }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.BottomGalleryItemSize)
            .Subscribe(x => { DockedGalleryItemSize.Value = x; }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.ExpandedGalleryItemSize)
            .Subscribe(x => { ExpandedGalleryItemSize.Value = x; }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.ItemSpacing)
            .Subscribe(x => { GalleryItemSpacing.Value = x; }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.LineSpacing)
            .Subscribe(x => { GalleryLineSpacing.Value = x; }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.BottomGalleryStretchMode)
            .Subscribe(x => { DockedGalleryStretchIndex.Value = GetStretchIndex(x); }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.FullGalleryStretchMode)
            .Subscribe(x => { FullGalleryStretchIndex.Value = GetStretchIndex(x); }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });


        DockedGalleryItemSize
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                if (Math.Abs(Settings.Gallery.BottomGalleryItemSize - x) > 0.001)
                {
                    Settings.Gallery.BottomGalleryItemSize = x;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        ExpandedGalleryItemSize
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                if (Math.Abs(Settings.Gallery.ExpandedGalleryItemSize - x) > 0.001)
                {
                    Settings.Gallery.ExpandedGalleryItemSize = x;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        GalleryItemSpacing
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                if (Math.Abs(Settings.Gallery.ItemSpacing - x) > 0.001)
                {
                    Settings.Gallery.ItemSpacing = x;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        GalleryLineSpacing
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                if (Math.Abs(Settings.Gallery.LineSpacing - x) > 0.001)
                {
                    Settings.Gallery.LineSpacing = x;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        DockedGalleryStretchIndex
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                var mode = GetStretchString(x);
                if (Settings.Gallery.BottomGalleryStretchMode != mode)
                {
                    Settings.Gallery.BottomGalleryStretchMode = mode;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        FullGalleryStretchIndex
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                var mode = GetStretchString(x);
                if (Settings.Gallery.FullGalleryStretchMode != mode)
                {
                    Settings.Gallery.FullGalleryStretchMode = mode;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });
    }

    private void ToggleGalleryVisibilitySubscription()
    {
#if DEBUG
        Debug.Assert(Settings?.Gallery is not null);
#endif

        CloseGalleryCommand.SubscribeAwait(async (_, ct) =>
        {
            await GalleryManager.CloseDockedGalleryAsync(ct);
        }, result =>
        {
#if DEBUG
            if (result is { IsFailure: true, Exception: not null })
            {
                DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
            }
#endif
        });

        SetDockPositionCommand.Subscribe(pos =>
        {
            Settings.Gallery.IsGalleryDocked = true;
            Settings.Gallery.DockPosition = pos;
        }, result =>
        {
#if DEBUG
            if (result is { IsFailure: true, Exception: not null })
            {
                DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
            }
#endif
        });

        Observable.EveryValueChanged(Settings.Gallery, x => x.IsGalleryDocked)
            .Skip(1)
            .Subscribe(x =>
            {
                IsGalleryDocked.Value = x;
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        IsGalleryDocked
            .Skip(1)
            .SubscribeAwait(async (isDocked, ct) =>
            {
                if (!isDocked)
                {
                    await GalleryManager.CloseDockedGalleryAsync(ct);
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        IsDockedGalleryShownInHiddenUI
            .Skip(1)
            .SubscribeAwait(async (x, _) =>
            {
                if (Settings.Gallery.ShowBottomGalleryInHiddenUI != x)
                {
                    Settings.Gallery.ShowBottomGalleryInHiddenUI = x;
                    await SaveSettingsAsync();
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });

        Observable.EveryValueChanged(Settings.Gallery, x => x.ShowBottomGalleryInHiddenUI)
            .Subscribe(x => { IsDockedGalleryShownInHiddenUI.Value = x; }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GallerySharedSettingsViewModel), nameof(Initialize),
                        result.Exception);
                }
#endif
            });
    }

    private void UpdateDockPositionProperties()
    {
        var pos = Settings.Gallery.DockPosition;
        IsTopDocked.Value = pos == GalleryDockPosition.Top;
        IsBottomDocked.Value = pos == GalleryDockPosition.Bottom;
        IsLeftDocked.Value = pos == GalleryDockPosition.Left;
        IsRightDocked.Value = pos == GalleryDockPosition.Right;
    }

    private static int GetStretchIndex(string mode)
    {
        return mode switch
        {
            "Uniform" => 0,
            "UniformToFill" => 1,
            "Fill" => 2,
            "None" => 3,
            "Square" => 4,
            "FillSquare" => 5,
            _ => 0
        };
    }

    private static string GetStretchString(int index)
    {
        return index switch
        {
            0 => "Uniform",
            1 => "UniformToFill",
            2 => "Fill",
            3 => "None",
            4 => "Square",
            5 => "FillSquare",
            _ => "Uniform"
        };
    }
}