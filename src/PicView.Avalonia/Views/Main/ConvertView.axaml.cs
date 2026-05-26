using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.CustomControls;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.Main;

public partial class ConvertView : UserControl
{
    // TODO: refactor to new image file saving system
    public ConvertView()
    {
        InitializeComponent();
        
        Loaded += delegate
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
        
            SaveButton.Click += async (_, _) =>
            {
                // var destination = vm.PicViewer.FileInfo.CurrentValue.FullName;
                // var ext = DetermineFileExtension(vm, ref destination);
                //
                // await SaveImageHandler.SaveImageWithPossibleNavigation(vm, vm.PicViewer.FileInfo.CurrentValue.FullName,
                //     destination, true, ext);
            };
        
            SaveAsButton.Click += async (_, _) =>
            {
                // var ext = GetExtension();
                // var destination =
                //     await FilePicker.PickFileForSavingAsync(vm.PicViewer.FileInfo?.CurrentValue.FullName, ext);
                // if (destination is null)
                // {
                //     return;
                // }
                //
                // var sameFile = destination.Equals(vm.PicViewer.FileInfo.CurrentValue.FullName,
                //     StringComparison.OrdinalIgnoreCase);
                //
                // await SaveImageHandler.SaveImageWithPossibleNavigation(vm, vm.PicViewer.FileInfo.CurrentValue.FullName,
                //     destination, sameFile, ext);
            };
        
            CancelButton.Click += (_, _) => SafeClose();
        };
    }
    
    private void SafeClose()
    {
        Dispatcher.Invoke(() =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window)
            {
                return;
            }
            window.Close();
        });
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

    private string DetermineFileExtension(MainWindowViewModel vm, ref string destination)
    {
        // var ext = vm.PicViewer.FileInfo.CurrentValue.Extension;
        // if (NoConversion.IsSelected)
        // {
        //     return ext;
        // }
        //
        // if (PngItem.IsSelected)
        // {
        //     ext = ".png";
        // }
        // else if (JpgItem.IsSelected)
        // {
        //     ext = ".jpg";
        // }
        // else if (WebpItem.IsSelected)
        // {
        //     ext = ".webp";
        // }
        // else if (AvifItem.IsSelected)
        // {
        //     ext = ".avif";
        // }
        // else if (HeicItem.IsSelected)
        // {
        //     ext = ".heic";
        // }
        // else if (JxlItem.IsSelected)
        // {
        //     ext = ".jxl";
        // }
        //
        // destination = Path.ChangeExtension(destination, ext);
        // return ext;
        return "";
    }

    private void BottomBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not GenericWindow window)
        {
            return;
        }
        window.MoveWindow(sender,e);
    }
}