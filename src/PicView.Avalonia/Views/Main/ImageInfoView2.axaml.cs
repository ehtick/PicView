using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PicView.Avalonia.Views.UC;
using PicView.Core.ViewModels;
using PicView.Core.Conversion;
using PicView.Core.Exif;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.Sizing;
using PicView.Core.Titles;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class ImageInfoView2 : UserControl
{
    private readonly CompositeDisposable _disposables = new();

    public ImageInfoView2()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            ResponsiveResizeUpdate(vm);

            KeyDown += (_, e) =>
            {
                switch (e.Key)
                {
                    case Key.Down:
                    case Key.PageDown:
                        ScrollViewer.LineDown();
                        break;
                    case Key.Up:
                    case Key.PageUp:
                        ScrollViewer.LineUp();
                        break;
                    case Key.Home:
                        ScrollViewer.ScrollToHome();
                        break;
                    case Key.End:
                        ScrollViewer.ScrollToEnd();
                        break;
                }
            };

            PointerPressed += (_, e) =>
            {
                if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                {
                    return;
                }

                // Context menu doesn't want to be opened normally
                MainContextMenu.Open();
            };

            CloseItem.Click += (_, _) => (VisualRoot as Window)?.Close();

            PixelWidthTextBox.KeyDown += async (s, e) => await ResizeImageOnEnter(s, e);
            PixelHeightTextBox.KeyDown += async (s, e) => await ResizeImageOnEnter(s, e);

            PixelWidthTextBox.KeyUp += delegate { AdjustAspectRatio(PixelWidthTextBox); };
            PixelHeightTextBox.KeyUp += delegate { AdjustAspectRatio(PixelHeightTextBox); };

            Observable.EveryValueChanged(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, x => x)
                .SubscribeAwait(UpdateValuesAsync).AddTo(_disposables);

            SizeChanged += (_, _) => ResponsiveResizeUpdate(vm);

            RemoveImageDataMenuItem.Click += async (_, _) => { await RemoveImageDataAsync(); };
            
            FileNameTextBox.KeyDown += async (_, e) =>
                await HandleRenameOnEnterAsync(e, () =>
                    Path.Combine(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo.DirectoryName!, FileNameTextBox.Text));

            FullPathTextBox.KeyDown += async (_, e) =>
                await HandleRenameOnEnterAsync(e, () => FullPathTextBox.Text ?? string.Empty);

            DirectoryNameTextBox.KeyDown += async (_, e) =>
                await HandleRenameOnEnterAsync(e, () =>
                    Path.Combine(DirectoryNameTextBox.Text, vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo.Name));

            // Register EXIF property updates on 'Enter' key press
            RegisterExifUpdateHandlers();
            
            // Orientation is for display only atm
            OrientationBox.DropDownClosed += (_, _) =>
            {
                OrientationBox.SelectedIndex = vm.Exif.Orientation.Value!;
            };
            
            // Resolution Units are for display only atm
            ResolutionUnitBox.DropDownClosed += (_, _) =>
            {
                ResolutionUnitBox.SelectedIndex = (int)vm.Exif.ResolutionUnit.Value!;
            };

            ColorRepresentationBox.DropDownClosed += async (_, _) =>
            {
                await AddExifPropertyAsync(ExifWriter.AddColorSpace, vm.Exif.ColorRepresentation.CurrentValue);
            };
            
            CompressionBox.DropDownClosed  += async (_, _) =>
            {
                await AddExifPropertyAsync(ExifWriter.AddCompression, vm.Exif.Compression.CurrentValue);
            };

            vm.InfoWindow.IsLoading.Value = false;
        };
    }

    private async Task HandleRenameOnEnterAsync(KeyEventArgs e, Func<string> getNewPath)
    {
        if (e.Key is not Key.Enter || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        try
        {
            var newPath = getNewPath();
            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(true));
            vm.IsLoadingIndicatorShown.Value = true;
            //NavigationManager.DisableWatcher();

            var fileInfo = vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo;
            var oldPath = fileInfo.FullName;

            // Avoid renaming if the path hasn't changed
            if (oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var currentExtension = Path.GetExtension(oldPath);
            var newExtension = Path.GetExtension(newPath);
            if (currentExtension.Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            {
                // Same file, handle simple rename

                // Make sure the old file is discarded from being cached
                //NavigationManager.RemoveFromPreloader(oldPath);

                FileHelper.RenameFile(oldPath, newPath);

                vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo = new FileInfo(newPath);
            }
            else
            {
                // Convert and reload
                // await SaveImageHandler.SaveImageWithPossibleNavigation(vm, vm.WindowTabs.ActiveTab.Value.FileInfo.CurrentValue.FullName,
                //     newPath, true, newExtension);
            }

            //await NavigationManager.QuickReload();

            await UpdateValuesAsync(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, CancellationToken.None);
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(false));
            vm.IsLoadingIndicatorShown.Value = false;
        }
    }


    private void ResponsiveResizeUpdate(MainWindowViewModel vm)
    {
        if (!Application.Current.TryGetResource("ScrollBarThickness", Application.Current.ActualThemeVariant,
                out var value))
        {
            return;
        }

        if (value is not double scrollBarThickness)
        {
            return;
        }

        var panelWidth = double.IsNaN(ParentPanel.Width) ? ParentPanel.Bounds.Width : ParentPanel.Width;
        panelWidth = panelWidth is 0 ? MinWidth : panelWidth;

        vm.InfoWindow.ResponsiveResizeUpdate(panelWidth, scrollBarThickness);
    }

    private async ValueTask UpdateValuesAsync(FileInfo? fileInfo, CancellationToken cancellationToken)
    {
        if (DataContext is not MainWindowViewModel vm || fileInfo is null)
        {
            return;
        }
        
        //var preLoadValue = await NavigationManager.GetPreLoadValueAsync(fileInfo);
        await Task.Run(() =>
        {
            //vm.Exif.UpdateExifValues(preLoadValue.ImageModel);
        }, cancellationToken);
        if (DirectoryNameTextBox.Text != fileInfo.DirectoryName)
        {
            DirectoryNameTextBox.Text = fileInfo.DirectoryName;
        }

        FileSizeBox.Text = vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo?.Length.GetReadableFileSize();
        // vm.WindowTabs.ActiveTab.Value.Model.ShouldOptimizeImageBeEnabled.Value =
        //     ConversionHelper.DetermineIfOptimizeImageShouldBeEnabled(vm.WindowTabs.ActiveTab.Value.Model.FileInfo?.CurrentValue);
        GoogleLinkButton.IsEnabled = !string.IsNullOrWhiteSpace(vm.Exif.GoogleLink.CurrentValue);
        BingLinkButton.IsEnabled = !string.IsNullOrWhiteSpace(vm.Exif.BingLink.CurrentValue);

        vm.Exif.IsExifAvailable.Value = vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.Format.IsExifImage();
    }



    private void SetLoadingState(bool isLoading)
    {
        ParentPanel.Opacity = isLoading ? 0.1 : 1;
        ParentPanel.IsHitTestVisible = !isLoading;
        SpinWaiter.IsVisible = isLoading;
    }



    private void AdjustAspectRatio(TextBox sender)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var aspectRatio = (double)vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.PixelWidth / vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.PixelHeight;
        // AspectRatioHelper.SetAspectRatioForTextBox(PixelWidthTextBox, PixelHeightTextBox, sender == PixelWidthTextBox,
        //     aspectRatio, DataContext as MainWindowViewModel);

        if (!int.TryParse(PixelWidthTextBox.Text, out var width) ||
            !int.TryParse(PixelHeightTextBox.Text, out var height))
        {
            return;
        }

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var printSizes =
            PrintSizing.GetPrintSizes(width, height, vm.Exif.DpiX.CurrentValue, vm.Exif.DpiY.CurrentValue);
        PrintSizeInchTextBox.Text = printSizes.PrintSizeInch;
        PrintSizeCmTextBox.Text = printSizes.PrintSizeCm;
        SizeMpTextBox.Text = printSizes.SizeMp;

        var gcd = AspectRatioFormatter.GCD(width, height);
        AspectRatioTextBox.Text =
            AspectRatioFormatter.GetFormattedAspectRatio(gcd, vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.PixelWidth,
                vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.PixelHeight);
    }

    private static async Task DoResize(MainWindowViewModel vm, bool isWidth, object width, object height)
    {
        if (isWidth)
        {
            if (!double.TryParse((string?)width, out var widthValue))
            {
                return;
            }

            if (widthValue > 0)
            {
                var success = await ConversionHelper.ResizeByWidth(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, widthValue)
                    .ConfigureAwait(false);
                if (success)
                {
                    //await NavigationManager.QuickReload().ConfigureAwait(false);
                }
            }
        }
        else
        {
            if (!double.TryParse((string?)height, out var heightValue))
            {
                return;
            }

            if (heightValue > 0)
            {
                var success = await ConversionHelper.ResizeByHeight(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, heightValue)
                    .ConfigureAwait(false);
                if (success)
                {
                    //await NavigationManager.QuickReload().ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ResizeImageOnEnter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(true));
            try
            {
                await DoResize(vm, Equals(sender, PixelWidthTextBox), PixelWidthTextBox.Text, PixelHeightTextBox.Text)
                    .ConfigureAwait(false);
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(false));
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Disposable.Dispose(_disposables);
    }

    #region EXIF Update Registration

    /// <summary>
    /// Helper method to register a KeyDown event for a TextBox to update an EXIF property.
    /// </summary>
    private void RegisterExifUpdateOnEnter(TextBox textBox, Func<Task> updateAction)
    {
        textBox.KeyDown += async (_, e) =>
        {
            if (e.Key is Key.Enter or Key.Tab)
            {
                await updateAction();
            }
        };
    }

    /// <summary>
    /// Registers all EXIF property update handlers.
    /// </summary>
    private void RegisterExifUpdateHandlers()
    {
        RegisterExifUpdateOnEnter(AuthorsBox, AddAuthorsAsync);
        RegisterExifUpdateOnEnter(CopyRightBox, AddCopyrightAsync);
        RegisterExifUpdateOnEnter(SoftwareBox, AddSoftwareAsync);
        RegisterExifUpdateOnEnter(SubjectBox, AddSubjectAsync);
        RegisterExifUpdateOnEnter(TitleBox, AddTitleAsync);
        RegisterExifUpdateOnEnter(CommentBox, AddCommentAsync);
        RegisterExifUpdateOnEnter(LatitudeBox, AddLatitudeAsync);
        RegisterExifUpdateOnEnter(LongitudeBox, AddLongitudeAsync);
        RegisterExifUpdateOnEnter(AltitudeBox, AddAltitudeAsync);
        RegisterExifUpdateOnEnter(CompressedBitsPerPixelBox, AddCompressedBitsPerPixelAsync);
        RegisterExifUpdateOnEnter(CameraMakerBox, AddCameraMakerAsync);
        RegisterExifUpdateOnEnter(CameraModelBox, AddCameraModelAsync);
        RegisterExifUpdateOnEnter(FNumberBox, AddFNumberAsync);
        RegisterExifUpdateOnEnter(MaxApertureBox, AddMaxApertureAsync);
        RegisterExifUpdateOnEnter(ExposureBiasBox, AddExposureBiasAsync);
        RegisterExifUpdateOnEnter(ExposureTimeBox, AddExposureTimeAsync);
        RegisterExifUpdateOnEnter(ExposureProgramBox, AddExposureProgramAsync);
        RegisterExifUpdateOnEnter(DigitalZoomBox, AddDigitalZoomAsync);
        RegisterExifUpdateOnEnter(FocalLengthBox, AddFocalLengthAsync);
        RegisterExifUpdateOnEnter(FocalLength35mmBox, AddFocalLength35mmAsync);
        RegisterExifUpdateOnEnter(IsoSpeedBox, AddIsoSpeedAsync);
        RegisterExifUpdateOnEnter(MeteringModeBox, AddMeteringModeAsync);
        RegisterExifUpdateOnEnter(ContrastBox, AddContrastAsync);
        RegisterExifUpdateOnEnter(SaturationBox, AddSaturationAsync);
        RegisterExifUpdateOnEnter(SharpnessBox, AddSharpnessAsync);
        RegisterExifUpdateOnEnter(WhiteBalanceBox, AddWhiteBalanceAsync);
        RegisterExifUpdateOnEnter(FlashEnergyBox, AddFlashEnergyAsync);
        RegisterExifUpdateOnEnter(FlashModeBox, AddFlashModeAsync);
        RegisterExifUpdateOnEnter(LightSourceBox, AddLightSourceAsync);
        RegisterExifUpdateOnEnter(BrightnessBox, AddBrightnessAsync);
        RegisterExifUpdateOnEnter(PhotometricInterpretationBox, AddPhotometricInterpretationAsync);
        RegisterExifUpdateOnEnter(LensMakerBox, AddLensMakerAsync);
        RegisterExifUpdateOnEnter(LensModelBox, AddLensModelAsync);
        RegisterExifUpdateOnEnter(ExifVersionBox, AddExifVersionAsync);
    }

    #endregion

    #region EXIF Update Methods

    /// <summary>
    /// Generic helper to add an EXIF property value to the image file.
    /// </summary>
    private async Task AddExifPropertyAsync<T>(Func<FileInfo?, T, Task<bool>> addAction, T value)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var isAdded = await addAction(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, value);
        if (isAdded)
        {
            await UpdateValuesAsync(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, CancellationToken.None);
        }
    }

    public async Task RemoveImageDataAsync()
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var isRemoved = await ExifWriter.RemoveExifProfile(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo);
        if (isRemoved)
        {
            await UpdateValuesAsync(vm.WindowTabs.ActiveTab.Value.Model.CurrentValue.FileInfo, CancellationToken.None);
        }
    }

    private async Task AddAuthorsAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddAuthors, vm.Exif.Authors.CurrentValue);
        }
    }

    private async Task AddCopyrightAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddCopyright, vm.Exif.Copyright.CurrentValue);
        }
    }

    private async Task AddSoftwareAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddSoftware, vm.Exif.Software.CurrentValue);
        }
    }

    private async Task AddSubjectAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddSubject, vm.Exif.Subject.CurrentValue);
        }
    }

    private async Task AddTitleAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddTitle, vm.Exif.Title.CurrentValue);
        }
    }

    private async Task AddCommentAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddComment, vm.Exif.Comment.CurrentValue);
        }
    }

    private async Task AddLatitudeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(GpsHelper.AddLatitude, vm.Exif.Latitude.CurrentValue);
        }
    }

    private async Task AddLongitudeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(GpsHelper.AddLongitude, vm.Exif.Longitude.CurrentValue);
        }
    }

    private async Task AddAltitudeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(GpsHelper.AddAltitude, vm.Exif.Altitude.CurrentValue);
        }
    }

    private async Task AddCompressionAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddCompression, vm.Exif.Compression.CurrentValue);
        }
    }

    private async Task AddCompressedBitsPerPixelAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddCompressedBitsPerPixel, vm.Exif.CompressedBitsPixel.CurrentValue);
        }
    }

    private async Task AddCameraMakerAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddCameraMaker, vm.Exif.CameraMaker.CurrentValue);
        }
    }

    private async Task AddCameraModelAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddCameraModel, vm.Exif.CameraModel.CurrentValue);
        }
    }

    private async Task AddFNumberAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddFNumber, vm.Exif.FNumber.CurrentValue);
        }
    }

    private async Task AddMaxApertureAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddMaxAperture, vm.Exif.MaxAperture.CurrentValue);
        }
    }

    private async Task AddExposureBiasAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddExposureBias, vm.Exif.ExposureBias.CurrentValue);
        }
    }

    private async Task AddExposureTimeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddExposureTime, vm.Exif.ExposureTime.CurrentValue);
        }
    }

    private async Task AddExposureProgramAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddExposureProgram, vm.Exif.ExposureProgram.CurrentValue);
        }
    }

    private async Task AddDigitalZoomAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddDigitalZoom, vm.Exif.DigitalZoom.CurrentValue);
        }
    }

    private async Task AddFocalLengthAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddFocalLength, vm.Exif.FocalLength.CurrentValue);
        }
    }

    private async Task AddFocalLength35mmAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddFocalLength35mm, vm.Exif.FocalLength35Mm.CurrentValue);
        }
    }

    private async Task AddIsoSpeedAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddIsoSpeed, vm.Exif.ISOSpeed.CurrentValue);
        }
    }

    private async Task AddMeteringModeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddMeteringMode, vm.Exif.MeteringMode.CurrentValue);
        }
    }

    private async Task AddContrastAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddContrast, vm.Exif.Contrast.CurrentValue);
        }
    }

    private async Task AddSaturationAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddSaturation, vm.Exif.Saturation.CurrentValue);
        }
    }

    private async Task AddSharpnessAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddSharpness, vm.Exif.Sharpness.CurrentValue);
        }
    }

    private async Task AddWhiteBalanceAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddWhiteBalance, vm.Exif.WhiteBalance.CurrentValue);
        }
    }

    private async Task AddFlashEnergyAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddFlashEnergy, vm.Exif.FlashEnergy.CurrentValue);
        }
    }

    private async Task AddFlashModeAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddFlashMode, vm.Exif.FlashMode.CurrentValue);
        }
    }

    private async Task AddLightSourceAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddLightSource, vm.Exif.LightSource.CurrentValue);
        }
    }

    private async Task AddBrightnessAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddBrightness, vm.Exif.Brightness.CurrentValue);
        }
    }

    private async Task AddPhotometricInterpretationAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddPhotometricInterpretation,
                vm.Exif.PhotometricInterpretation.CurrentValue);
        }
    }

    private async Task AddLensMakerAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddLensMaker, vm.Exif.LensMaker.CurrentValue);
        }
    }

    private async Task AddLensModelAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddLensModel, vm.Exif.LensModel.CurrentValue);
        }
    }

    private async Task AddExifVersionAsync()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await AddExifPropertyAsync(ExifWriter.AddExifVersion, vm.Exif.ExifVersion.CurrentValue);
        }
    }

    #endregion
}