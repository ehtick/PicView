using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.Converters;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Resizing;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ImageDecoding;
using PicView.Core.Titles;
using R3;

namespace PicView.Avalonia.Views;

public partial class ImageInfoView : UserControl
{
    private readonly CompositeDisposable _disposables = new();
    public ImageInfoView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
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

            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            Observable.EveryValueChanged(vm.PicViewer.FileInfo, x => x.Value, UIHelper.GetFrameProvider).Subscribe(x =>
            {
                ExifHandling.UpdateExifValues(vm);
                if (DirectoryNameTextBox.Text != x.DirectoryName)
                {
                    DirectoryNameTextBox.Text = x.DirectoryName;
                }
            }).AddTo(_disposables);
            
            ResetButton.Click += (_, _) =>
            {
                PixelWidthTextBox.Text = vm.PicViewer.PixelWidth.ToString();
                PixelHeightTextBox.Text = vm.PicViewer.PixelHeight.ToString();
                AdjustAspectRatio(PixelWidthTextBox);
                FullPathTextBox.Text = vm.PicViewer.FileInfo?.CurrentValue.FullName ?? "";
                DirectoryNameTextBox.Text = vm.PicViewer.FileInfo?.CurrentValue.DirectoryName ?? "";
                FileNameTextBox.Text = vm.PicViewer.FileInfo?.CurrentValue.Name ?? "";
            };

            SaveButton.Click += async (_, _) =>
            {
                var ext = GetExtension();
                var location = FullPathTextBox.Text; // TODO check if this is a valid path
                // and sync with file name/directory text boxes
                await SendToImageSaver(vm.PicViewer.FileInfo?.CurrentValue.FullName, location, PixelWidthTextBox.Text,
                    PixelHeightTextBox.Text, ext).ConfigureAwait(false);
            };

            SaveAsButton.Click += async (_, _) =>
            {
                var fileInfoFullName = vm.PicViewer.FileInfo.CurrentValue.FullName;
                var ext = DetermineFileExtension(vm, ref fileInfoFullName);

                var file = await FilePicker.PickFileForSavingAsync(vm.PicViewer.FileInfo?.CurrentValue.FullName, ext);
                if (file is null)
                {
                    return;
                }

                await SendToImageSaver(vm.PicViewer.FileInfo?.CurrentValue.FullName, file, PixelWidthTextBox.Text,
                    PixelHeightTextBox.Text, ext).ConfigureAwait(false);
            };
            FileNameTextBox.KeyDown += async (_, e) =>
            {
                if (e.Key is not Key.Enter)
                {
                    return;
                }

                var newPath = Path.Combine(vm.PicViewer.FileInfo.CurrentValue.DirectoryName, FileNameTextBox.Text);
                var oldPath = vm.PicViewer.FileInfo.CurrentValue.FullName;
                var renamed = await FileRenamer.AttemptRenameAsync(oldPath, newPath, vm).ConfigureAwait(false);
                if (renamed)
                {
                    await NavigationManager.LoadPicFromFile(newPath, vm).ConfigureAwait(false);
                }
            };
            FullPathTextBox.KeyDown += async (_, e) =>
            {
                if (e.Key is not Key.Enter)
                {
                    return;
                }

                var newPath = FullPathTextBox.Text;
                var oldPath = vm.PicViewer.FileInfo.CurrentValue.FullName;
                var renamed = await FileRenamer.AttemptRenameAsync(oldPath, newPath, vm).ConfigureAwait(false);
                if (renamed)
                {
                    await NavigationManager.LoadPicFromFile(newPath, vm).ConfigureAwait(false);
                }
            };
            DirectoryNameTextBox.KeyDown += async (_, e) =>
            {
                if (e.Key is not Key.Enter)
                {
                    return;
                }

                var oldDirectory = vm.PicViewer.FileInfo.CurrentValue.DirectoryName;
                var newDirectory = DirectoryNameTextBox.Text;

                var oldPath = vm.PicViewer.FileInfo.CurrentValue.FullName;
                var newPath = oldPath.Replace(oldDirectory, newDirectory);

                await FileRenamer.AttemptRenameAsync(oldPath, newPath, vm).ConfigureAwait(false);
            };
        };
    }

    private async Task SendToImageSaver(string? location, string destination, string? width, string? height,
        string ext)
    {
        if (!uint.TryParse(width, out var widthValue) || !uint.TryParse(height, out var heightValue))
        {
            return;
        }

        if (ext != Path.GetExtension(location))
        {
            destination = Path.ChangeExtension(destination, ext);
            await FileRenamer.AttemptRenameAsync(location, destination, DataContext as MainViewModel,
                widthValue, heightValue);
        }
        else
        {
            await SaveImageFileHelper.SaveImageAsync(null, location, destination, widthValue, heightValue, null, ext)
                .ConfigureAwait(false);
        }
    }

    private string GetExtension() => ConversionComboBox.SelectedIndex switch
    {
        1 => ".png",
        2 => ".jpg",
        3 => ".webp",
        4 => ".avif",
        5 => ".heic",
        6 => ".jxl",
        _ => ""
    };

    private void AdjustAspectRatio(TextBox sender)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        var aspectRatio = (double)vm.PicViewer.PixelWidth.CurrentValue / vm.PicViewer.PixelHeight.CurrentValue;
        AspectRatioHelper.SetAspectRatioForTextBox(PixelWidthTextBox, PixelHeightTextBox, sender == PixelWidthTextBox,
            aspectRatio, DataContext as MainViewModel);

        if (!int.TryParse(PixelWidthTextBox.Text, out var width) ||
            !int.TryParse(PixelHeightTextBox.Text, out var height))
        {
            return;
        }

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var printSizes = AspectRatioHelper.GetPrintSizes(width, height, vm.Exif.DpiX.CurrentValue, vm.Exif.DpiY.CurrentValue);
        PrintSizeInchTextBox.Text = printSizes.PrintSizeInch;
        PrintSizeCmTextBox.Text = printSizes.PrintSizeCm;
        SizeMpTextBox.Text = printSizes.SizeMp;

        var gcd = ImageTitleFormatter.GCD(width, height);
        AspectRatioTextBox.Text =
            AspectRatioHelper.GetFormattedAspectRatio(gcd, vm.PicViewer.PixelWidth.CurrentValue,
                vm.PicViewer.PixelHeight.CurrentValue);
    }

    private static async Task DoResize(MainViewModel vm, bool isWidth, object width, object height)
    {
        if (isWidth)
        {
            if (!double.TryParse((string?)width, out var widthValue))
            {
                return;
            }

            if (widthValue > 0)
            {
                var success = await ConversionHelper.ResizeByWidth(vm.PicViewer.FileInfo.CurrentValue, widthValue)
                    .ConfigureAwait(false);
                if (success)
                {
                    await NavigationManager.QuickReload().ConfigureAwait(false);
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
                var success = await ConversionHelper.ResizeByHeight(vm.PicViewer.FileInfo.CurrentValue, heightValue)
                    .ConfigureAwait(false);
                if (success)
                {
                    await NavigationManager.QuickReload().ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ResizeImageOnEnter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            await DoResize(vm, Equals(sender, PixelWidthTextBox), PixelWidthTextBox.Text, PixelHeightTextBox.Text)
                .ConfigureAwait(false);
        }
    }

    private string DetermineFileExtension(MainViewModel vm, ref string destination)
    {
        var ext = vm.PicViewer.FileInfo.CurrentValue.Extension;
        if (NoConversion.IsSelected)
        {
            return ext;
        }

        if (PngItem.IsSelected)
        {
            ext = ".png";
        }
        else if (JpgItem.IsSelected)
        {
            ext = ".jpg";
        }
        else if (WebpItem.IsSelected)
        {
            ext = ".webp";
        }
        else if (AvifItem.IsSelected)
        {
            ext = ".avif";
        }
        else if (HeicItem.IsSelected)
        {
            ext = ".heic";
        }
        else if (JxlItem.IsSelected)
        {
            ext = ".jxl";
        }

        destination = Path.ChangeExtension(destination, ext);
        return ext;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Disposable.Dispose(_disposables);
    }
}