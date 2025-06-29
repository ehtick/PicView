using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Core.ImageEffects;
using ReactiveUI;
using Timer = System.Timers.Timer;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Avalonia.Views;

/// <summary>
/// User control that provides an interface for applying and managing image effects.
/// Allows users to adjust brightness, contrast, and other visual effects for displayed images.
/// </summary>
public partial class EffectsView : UserControl
{
    private readonly ImageEffectConfig _defaultEffectConfig = new();
    private readonly CompositeDisposable _disposables = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private Timer? _debounceTimer;
    private bool _reloading;

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectsView"/> class.
    /// Sets up event handlers for control lifecycle events.
    /// </summary>
    public EffectsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DetachedFromLogicalTree += OnDetachedFromLogicalTree;
    }

    /// <summary>
    /// Handles the control's Loaded event.
    /// Initializes the view model, UI event handlers, and debounce timer.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnLoaded(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        InitializeViewModel(vm);
        InitializeUIEvents(vm);
        InitializeDebounceTimer();
    }

    /// <summary>
    /// Initializes and configures the view model.
    /// Sets up reactive properties and applies effect configurations.
    /// </summary>
    /// <param name="vm">The main view model to initialize.</param>
    private void InitializeViewModel(MainViewModel vm)
    {
        // Reset on file change
        vm.ObservableForProperty(v => v.PicViewer.FileInfo)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Reset())
            .DisposeWith(_disposables);

        if (vm.PicViewer.EffectConfig == null)
        {
            vm.PicViewer.EffectConfig.Value = new ImageEffectConfig();
            HideResetBtn();
        }
        else if (IsDefaultEffectConfig(vm.PicViewer.EffectConfig.CurrentValue))
        {
            HideResetBtn();
        }
        else
        {
            ApplyEffectConfig(vm.PicViewer.EffectConfig.CurrentValue);
        }
    }

    /// <summary>
    /// Initializes UI event handlers for buttons and sliders.
    /// </summary>
    /// <param name="vm">The main view model to use for event handling.</param>
    private void InitializeUIEvents(MainViewModel vm)
    {
        CloseItem.Click += (_, _) => (VisualRoot as Window)?.Close();

        PointerPressed += OnPointerPressed;
        ClearEffectsItem.Click += async (_, _) => await RemoveEffects();
        ResetContrastBtn.Click += (_, _) => ContrastSlider.Value = 0;
        ResetBrightnessBtn.Click += (_, _) => BrightnessSlider.Value = 0;
        ResetPencilSketchBtn.Click += (_, _) => PencilSketchSlider.Value = 0;
        ResetPosterizeBtn.Click += (_, _) => PosterizeSlider.Value = 0;
        ResetSolarizeBtn.Click += (_, _) => SolarizeSlider.Value = 0;
        ResetBlurBtn.Click += (_, _) => BlurSlider.Value = 0;

        ResetButton.Click += async (_, _) => await RemoveEffects();
        CancelButton.Click += (_, _) => (VisualRoot as Window)?.Close();

        BrightnessSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Brightness = new Percentage(e.NewValue));
            HideCancelBtn();
        };
        ContrastSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Contrast = new Percentage(e.NewValue));
            HideCancelBtn();
        };
        PencilSketchSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.SketchStrokeWidth = e.NewValue);
            HideCancelBtn();
        };
        PosterizeSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.PosterizeLevel = (int)e.NewValue == 1 ? 2 : (int)e.NewValue);
            HideCancelBtn();
        };
        SolarizeSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Solarize = new Percentage(e.NewValue));
            HideCancelBtn();
        };
        BlurSlider.ValueChanged += (s, e) =>
        {
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.BlurLevel = e.NewValue);
            HideCancelBtn();
        };

        BlackAndWhiteToggleButton.Click += async delegate
        {
            var isBlackAndWhite = BlackAndWhiteToggleButton.IsChecked.HasValue &&
                                  BlackAndWhiteToggleButton.IsChecked.Value;
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, BlackAndWhiteToggleButton, config => config.BlackAndWhite = isBlackAndWhite);
            HideCancelBtn();
        };
        NegativeToggleButton.Click += async delegate
        {
            var isNegative = NegativeToggleButton.IsChecked.HasValue && NegativeToggleButton.IsChecked.Value;
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, NegativeToggleButton, config => config.Negative = isNegative);
            HideCancelBtn();
        };
        OldMovieToggleButton.Click += async delegate
        {
            var isOldMovie = OldMovieToggleButton.IsChecked.HasValue && OldMovieToggleButton.IsChecked.Value;
            vm.PicViewer.EffectConfig.Value ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, OldMovieToggleButton, config => config.OldMovie = isOldMovie);
            HideCancelBtn();
        };
    }

    /// <summary>
    /// Hides the reset button and shows the cancel button.
    /// Used when the effect configuration is at default settings.
    /// </summary>
    private void HideResetBtn()
    {
        ResetButton.IsVisible = false;
        CancelButton.IsVisible = true;
    }

    /// <summary>
    /// Shows the reset button and hides the cancel button.
    /// Used when effects have been applied and can be reset.
    /// </summary>
    private void HideCancelBtn()
    {
        ResetButton.IsVisible = true;
        CancelButton.IsVisible = false;
    }

    /// <summary>
    /// Handles pointer press events to show context menu on right-click.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing pointer information.</param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            ContextMenu.Open();
        }
    }

    /// <summary>
    /// Initializes the debounce timer used to delay applying effects until user input has stopped.
    /// </summary>
    private void InitializeDebounceTimer()
    {
        _debounceTimer = new Timer { Interval = 300, AutoReset = false };
        _debounceTimer.Elapsed += async (_, _) => await ApplyEffectsDebounced();
    }

    /// <summary>
    /// Restarts the debounce timer to delay effect application.
    /// </summary>
    private void DebounceSliderChange()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    /// <summary>
    /// Applies image effects after the debounce period has elapsed.
    /// Cancels any pending effect application operations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ApplyEffectsDebounced()
    {
        if (_reloading)
        {
            return;
        }

        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        _cancellationTokenSource = new CancellationTokenSource();

        MainViewModel? vm = null;
        await Dispatcher.UIThread.InvokeAsync(() => { vm = DataContext as MainViewModel; });

        vm.IsLoading = true;

        try
        {
            var magick = await ImageEffectsHelper
                .ApplyEffects(vm.PicViewer.FileInfo.CurrentValue, vm.PicViewer.EffectConfig.CurrentValue, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
            if (magick is not null)
            {
                vm.PicViewer.ImageSource.Value = magick.ToWriteableBitmap();
            }
        }
        finally
        {
            vm.IsLoading = false;
        }
    }

    /// <summary>
    /// Removes all effects from the current image and resets the UI.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RemoveEffects()
    {
        _reloading = true;
        try
        {
            await NavigationManager.QuickReload().ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(Reset);
        }
        finally
        {
            _reloading = false;
        }
    }

    /// <summary>
    /// Resets all UI controls to their default values and clears effect configuration.
    /// </summary>
    private void Reset()
    {
        HideResetBtn();
        BlackAndWhiteToggleButton.IsChecked = false;
        NegativeToggleButton.IsChecked = false;
        OldMovieToggleButton.IsChecked = false;
        ContrastSlider.Value = 0;
        BrightnessSlider.Value = 0;
        PencilSketchSlider.Value = 0;
        PosterizeSlider.Value = 0;
        SolarizeSlider.Value = 0;
        BlurSlider.Value = 0;
        if (DataContext is MainViewModel vm)
        {
            vm.PicViewer.EffectConfig.Value = new ImageEffectConfig();
        }
    }

    /// <summary>
    /// Applies an effect configuration to the UI controls.
    /// </summary>
    /// <param name="config">The effect configuration to apply.</param>
    private void ApplyEffectConfig(ImageEffectConfig config)
    {
        if (config.BlackAndWhite)
        {
            BlackAndWhiteToggleButton.IsChecked = true;
        }

        if (config.OldMovie)
        {
            OldMovieToggleButton.IsChecked = true;
        }

        if (config.Negative)
        {
            NegativeToggleButton.IsChecked = true;
        }

        BrightnessSlider.Value = config.Brightness.ToInt32();
        ContrastSlider.Value = config.Contrast.ToInt32();
        PencilSketchSlider.Value = config.SketchStrokeWidth;
        PosterizeSlider.Value = config.PosterizeLevel;
        SolarizeSlider.Value = config.Solarize.ToInt32();
        BlurSlider.Value = config.BlurLevel;

        HideCancelBtn();
    }

    /// <summary>
    /// Updates the effect configuration and triggers a debounced effect application.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="updateAction">An action that updates the effect configuration.</param>
    private void UpdateEffectConfig(MainViewModel vm, Action<ImageEffectConfig> updateAction)
    {
        updateAction(vm.PicViewer.EffectConfig.CurrentValue);
        DebounceSliderChange();
    }

    /// <summary>
    /// Updates a toggle effect in the effect configuration and applies the effects.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="toggleButton">The toggle button that was clicked.</param>
    /// <param name="updateAction">An action that updates the effect configuration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateToggleEffect(MainViewModel vm, ToggleButton toggleButton,
        Action<ImageEffectConfig> updateAction)
    {
        var shouldReturn = false;
        await Dispatcher.UIThread.InvokeAsync(() => shouldReturn = !toggleButton.IsChecked.HasValue);
        if (shouldReturn)
        {
            return;
        }

        updateAction(vm.PicViewer.EffectConfig.CurrentValue);
        await ApplyEffectsDebounced();
    }

    /// <summary>
    /// Handles the control being detached from the logical tree.
    /// Cleans up resources to prevent memory leaks.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        CleanUp();
    }

    /// <summary>
    /// Finalizer that ensures resources are cleaned up.
    /// </summary>
    ~EffectsView()
    {
        CleanUp();
    }

    /// <summary>
    /// Cleans up resources used by this control.
    /// Disposes of disposables, timers, and cancellation tokens.
    /// </summary>
    private void CleanUp()
    {
        if (DataContext is MainViewModel vm)
        {
            vm.IsLoading = false;
        }

        _disposables.Dispose();
        _debounceTimer?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// Determines if an effect configuration matches the default configuration.
    /// </summary>
    /// <param name="config">The effect configuration to check.</param>
    /// <returns>True if the configuration matches the default; otherwise, false.</returns>
    private bool IsDefaultEffectConfig(ImageEffectConfig config) =>
        config.Brightness == _defaultEffectConfig.Brightness &&
        config.Contrast == _defaultEffectConfig.Contrast &&
        config.SketchStrokeWidth == _defaultEffectConfig.SketchStrokeWidth &&
        config.PosterizeLevel == _defaultEffectConfig.PosterizeLevel &&
        config.Solarize == _defaultEffectConfig.Solarize &&
        config.BlurLevel == _defaultEffectConfig.BlurLevel &&
        config.BlackAndWhite == _defaultEffectConfig.BlackAndWhite &&
        config.Negative == _defaultEffectConfig.Negative &&
        config.OldMovie == _defaultEffectConfig.OldMovie;
}