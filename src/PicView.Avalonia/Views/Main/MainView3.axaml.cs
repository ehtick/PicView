using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Crop;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Conversion;
using PicView.Core.ViewModels;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Views.Main;

public partial class MainView3 : UserControl
{
    public MainView3()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Move alt hover to left side on macOS and switch button order
            AltButtonsPanel.HorizontalAlignment = HorizontalAlignment.Left; 
            AltButtonsPanel.Children.Move(AltButtonsPanel.Children.IndexOf(AltClose),0);
            AltButtonsPanel.Children.Move(AltButtonsPanel.Children.IndexOf(AltMinimize),2);
            AltMinimize.RenderTransform = new ScaleTransform{ScaleX = -1};
        }


        Loaded += delegate
        {
            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            AddHandler(DragDrop.DropEvent, Drop);

            GotFocus += CloseTitlebarIfOpen;
            LostFocus += HandleLostFocus;
            PointerPressed += PointerPressedBehavior;

            if (Resources.TryGetResource("MainContextMenu", Application.Current.ActualThemeVariant, out var value))
            {
                if (value is ContextMenu mainContextMenu)
                {
                    mainContextMenu.Opened += async (sender, args) =>  await OnMainContextMenuOpened(sender, args);
                }
            }
            
            //MainTabControl.TabDetached += MainTabControlOnTabDetached;
            MainTabControl.TabCreated += MainTabControlOnTabCreated;
            MainTabControl.SelectionChanged += MainTabControlOnSelectionChanged;

            // Setup hover fade buttons
            //_ = new HoverFadeButtonHandler(AltButtonsPanel, vm);
        };
    }
    
    private void MainTabControlOnTabCreated(object? sender, TabCreatedEventArgs e)
    {
        // Only set the StartUpMenu if the View is currently null.
        // This prevents overwriting the view (e.g. an image) when reordering tabs,
        // as reordering triggers the TabCreated event again by recreating containers.
        if (e.CreatedItem is not TabViewModel { CurrentView.Value: null } tabViewModel)
        {
            return;
        }

        if (tabViewModel.Model.FileInfo is not null)
        {
            tabViewModel.CurrentView.Value = new ImageViewer2();
        }
        else
        {
            tabViewModel.CurrentView.Value = new StartUpMenu();
        }
    }
    private void MainTabControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (e.AddedItems[0] is not TabViewModel tab)
        {
            return;
        }

        vm.WindowTabs.SelectTab(tab);
        tab.UpdateTabTitle();
        tab.ImageIterator.UpdateNavigationProperties();
    }

    private void PointerPressedBehavior(object? sender, PointerPressedEventArgs e)
    {
        CloseTitlebarIfOpen(sender, e);
        if (MainKeyboardShortcuts.ShiftDown && !CropFunctions.IsCropping)
        {
            var hostWindow = (Window)VisualRoot!;
            WindowFunctions.WindowDragBehavior(hostWindow, e);
        }
        
        DragAndDropHelper.RemoveDragDropView();
    }
    
    private void CloseTitlebarIfOpen(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!vm.MainWindow.IsEditableTitlebarOpen.Value)
        {
            return;
        }

        vm.MainWindow.IsEditableTitlebarOpen.Value = false;
        MainKeyboardShortcuts.IsKeysEnabled = true;
        Focus();
    }
    
    private static void HandleLostFocus(object? sender, EventArgs e)
    {
        DragAndDropHelper.RemoveDragDropView();
    }

    private async Task OnMainContextMenuOpened(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.PicViewer.ShouldOptimizeImageBeEnabled.Value = ConversionHelper.DetermineIfOptimizeImageShouldBeEnabled(vm.PicViewer.FileInfo?.CurrentValue);

            // Set source for ChangeCtrlZoomImage
            if (!Application.Current.TryGetResource("ScanEyeImage", Application.Current.RequestedThemeVariant, out var scanEyeImage))
            {
                return;
            }
            if (!Application.Current.TryGetResource("LeftRightArrowsImage", Application.Current.RequestedThemeVariant, out var leftRightArrowsImage))
            {
                return;
            }
            var isNavigatingWithCtrl = Settings.Zoom.CtrlZoom;
            vm.MainWindow.ChangeCtrlZoomImage.Value = isNavigatingWithCtrl ? leftRightArrowsImage as DrawingImage : scanEyeImage as DrawingImage;
        });
        
    }

    private async Task Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        await DragAndDropHelper.Drop(e, vm);
    }
    
    private async Task DragEnter(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        await DragAndDropHelper.DragEnter(e, vm, this);
    }
    
    private void DragLeave(object? sender, DragEventArgs e)
    {
        DragAndDropHelper.DragLeave(e, this);
    }
}