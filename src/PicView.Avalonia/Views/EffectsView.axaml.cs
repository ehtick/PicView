using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageEffects;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace PicView.Avalonia.Views;

public partial class EffectsView : UserControl
{
    private CancellationTokenSource? _cancellationTokenSource;
    private Timer? _debounceTimer;
    private bool _reloading;
    private readonly CompositeDisposable _disposables = new();

    public EffectsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DetachedFromLogicalTree += OnDetachedFromLogicalTree;
    }

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

    private void InitializeViewModel(MainViewModel vm)
    {
        // Reset on file change
        vm.ObservableForProperty(v => v.FileInfo)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Reset())
            .DisposeWith(_disposables);

        if (vm.EffectConfig is null)
        {
            vm.EffectConfig = new ImageEffectConfig();
        }
        else
        {
            ApplyEffectConfig(vm.EffectConfig);
        }
    }

    private void InitializeUIEvents(MainViewModel vm)
    {
        PointerPressed += OnPointerPressed;
        ClearEffectsItem.Click += async (_, _) => await RemoveEffects(vm);
        ResetContrastBtn.Click += (_, _) => ContrastSlider.Value = 0;
        ResetBrightnessBtn.Click += (_, _) => BrightnessSlider.Value = 0;
        ResetPencilSketchBtn.Click += (_, _) => PencilSketchSlider.Value = 0;
        ResetPosterizeBtn.Click += (_, _) => PosterizeSlider.Value = 0;
        ResetSolarizeBtn.Click += (_, _) => SolarizeSlider.Value = 0;
        ResetBlurBtn.Click += (_, _) => BlurSlider.Value = 0;

        BrightnessSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Brightness = new Percentage(e.NewValue));
        };
        ContrastSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Contrast = new Percentage(e.NewValue));
        };
        PencilSketchSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.SketchStrokeWidth = e.NewValue);
        };
        PosterizeSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.PosterizeLevel = (int)e.NewValue == 1 ? 2 : (int)e.NewValue);  
        };
        SolarizeSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.Solarize = new Percentage(e.NewValue));
        };
        BlurSlider.ValueChanged += (s, e) =>
        {
            vm.EffectConfig ??= new ImageEffectConfig();
            UpdateEffectConfig(vm, config => config.BlurLevel = e.NewValue);
        };

        BlackAndWhiteToggleButton.Click += async delegate
        {
            var isBlackAndWhite = BlackAndWhiteToggleButton.IsChecked.HasValue && BlackAndWhiteToggleButton.IsChecked.Value;
            vm.EffectConfig ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, BlackAndWhiteToggleButton, config => config.BlackAndWhite = isBlackAndWhite);
        };
        NegativeToggleButton.Click += async delegate
        {
            var isNegative = NegativeToggleButton.IsChecked.HasValue && NegativeToggleButton.IsChecked.Value;
            vm.EffectConfig ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, NegativeToggleButton, config => config.Negative = isNegative);
        };
        OldMovieToggleButton.Click += async delegate
        {
            var isOldMovie = OldMovieToggleButton.IsChecked.HasValue && OldMovieToggleButton.IsChecked.Value;
            vm.EffectConfig ??= new ImageEffectConfig();
            await UpdateToggleEffect(vm, OldMovieToggleButton, config => config.OldMovie = isOldMovie);
        };
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            ContextMenu.Open();
        }
    }

    private void InitializeDebounceTimer()
    {
        _debounceTimer = new Timer { Interval = 300, AutoReset = false };
        _debounceTimer.Elapsed += async (_, _) => await ApplyEffectsDebounced();
    }

    private void DebounceSliderChange()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

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
        
        await ImageEffectsHelper.ApplyEffects(vm, vm.EffectConfig, _cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public async Task RemoveEffects(MainViewModel vm)
    {
        _reloading = true;
        try
        {
            await ErrorHandling.ReloadImageAsync(vm).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(Reset);
        }
        finally
        {
            _reloading = false;
        }
    }

    private void Reset()
    {
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
            vm.EffectConfig = new ImageEffectConfig();
        }
    }

    private void ApplyEffectConfig(ImageEffectConfig config)
    {
        if (config.BlackAndWhite) { BlackAndWhiteToggleButton.IsChecked = true; }
        if (config.OldMovie) { OldMovieToggleButton.IsChecked = true; }
        if (config.Negative) { NegativeToggleButton.IsChecked = true; }

        BrightnessSlider.Value = config.Brightness.ToInt32();
        ContrastSlider.Value = config.Contrast.ToInt32();
        PencilSketchSlider.Value = config.SketchStrokeWidth;
        PosterizeSlider.Value = config.PosterizeLevel;
        SolarizeSlider.Value = config.Solarize.ToInt32();
        BlurSlider.Value = config.BlurLevel;
    }

    private void UpdateEffectConfig(MainViewModel vm, Action<ImageEffectConfig> updateAction)
    {
        updateAction(vm.EffectConfig);
        DebounceSliderChange();
    }

    private async Task UpdateToggleEffect(MainViewModel vm, ToggleButton toggleButton, Action<ImageEffectConfig> updateAction)
    {
        var shouldReturn = false;
        await Dispatcher.UIThread.InvokeAsync(() => shouldReturn = !toggleButton.IsChecked.HasValue);
        if (shouldReturn)
        {
            return;
        }
        updateAction(vm.EffectConfig);
        await ApplyEffectsDebounced();
    }

    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        CleanUp();
    }

    ~EffectsView()
    {
        CleanUp();
    }

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
}