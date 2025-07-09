using System.Reactive;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PicView.Avalonia.Functions;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileSorting;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using ReactiveUI;
using ImageViewer = PicView.Avalonia.Views.ImageViewer;

namespace PicView.Avalonia.ViewModels;

public class MainViewModel : ReactiveObject
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    public TranslationViewModel Translation { get; } = new();
    public MainWindowViewModel MainWindow { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public ImageCropperViewModel? Crop { get; set; }
    public NavigationViewModel Navigation { get; } = new();
    public PicViewerModel PicViewer { get; } = new();
    public GalleryViewModel Gallery { get; } = new();
    public ToolsViewModel Tools { get; } = new();
    public ExifViewModel? Exif { get; set;  }
    public FileAssociationsViewModel? AssociationsViewModel { get; set; }

    public MainViewModel(IPlatformSpecificService? platformSpecificService, IPlatformWindowService? platformWindowService)
    {
        FunctionsMapper.Vm = this;
        PlatformService = platformSpecificService;
        PlatformWindowService = platformWindowService;

        #region Window commands

        ExitCommand = FunctionsHelper.CreateReactiveCommand(WindowFunctions.Close);
        MinimizeCommand = FunctionsHelper.CreateReactiveCommand(WindowFunctions.Minimize);
        MaximizeCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Maximize);
        RestoreCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Restore);
        ToggleFullscreenCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleFullscreen);
        NewWindowCommand = FunctionsHelper.CreateReactiveCommand(ProcessHelper.StartNewProcess);

        ShowExifWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowExifWindow);
        ShowSettingsWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowSettingsWindow);
        ShowKeybindingsWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowKeybindingsWindow);
        ShowAboutWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowAboutWindow);
        ShowBatchResizeWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowBatchResizeWindow);
        ShowSingleImageResizeWindowCommand =
            FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowSingleImageResizeWindow);
        ShowEffectsWindowCommand = FunctionsHelper.CreateReactiveCommand(PlatformWindowService.ShowEffectsWindow);

        #endregion Window commands

        #region Sort Commands

        SortFilesByNameCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesByName);

        SortFilesByCreationTimeCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesByCreationTime);

        SortFilesByLastAccessTimeCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesByLastAccessTime);

        SortFilesBySizeCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesBySize);

        SortFilesByExtensionCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesByExtension);

        SortFilesRandomlyCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesRandomly);

        SortFilesAscendingCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesAscending);

        SortFilesDescendingCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SortFilesDescending);

        #endregion Sort Commands

        #region Menus

        CloseMenuCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.CloseMenus);

        ToggleFileMenuCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleFileMenu);

        ToggleImageMenuCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleImageMenu);

        ToggleSettingsMenuCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleSettingsMenu);

        ToggleToolsMenuCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleToolsMenu);

        #endregion Menus

        #region Image commands

        RotateLeftCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.RotateLeft);
        RotateLeftButtonCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await RotationNavigation.RotateLeft(this, RotationButton.RotateLeftButton);
        });

        RotateRightCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.RotateRight);
        RotateRightButtonCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await RotationNavigation.RotateRight(this, RotationButton.RotateRightButton);
        });

        RotateRightWindowBorderButtonCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await RotationNavigation.RotateRight(this, RotationButton.WindowBorderButton);
        });

        FlipCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Flip);

        StretchCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Stretch);

        CropCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Crop);

        ToggleScrollCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleScroll);

        OptimizeImageCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.OptimizeImage);

        ChangeBackgroundCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ChangeBackground);

        ShowSideBySideCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SideBySide);

        #endregion Image commands

        #region UI Commands

        ToggleUICommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleInterface);

        ToggleBottomNavBarCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleBottomToolbar);

        ToggleBottomGalleryShownInHiddenUICommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await HideInterfaceLogic.ToggleBottomGalleryShownInHiddenUI(this);
        });

        ToggleFadeInButtonsOnHoverCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await HideInterfaceLogic.ToggleFadeInButtonsOnHover(this);
        });

        ChangeCtrlZoomCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ChangeCtrlZoom);

        ToggleTaskbarProgressCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleTaskbarProgress);
        
        ToggleConstrainBackgroundColorCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleConstrainBackgroundColor);

        #endregion UI Commands

        #region Settings commands

        ChangeAutoFitCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.AutoFitWindow);

        ChangeTopMostCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SetTopMost);

        ToggleSubdirectoriesCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleSubdirectories);

        ToggleLoopingCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleLooping);

        ResetSettingsCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ResetSettings);

        RestartCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Restart);
        
        ToggleOpeningInSameWindowCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleOpeningInSameWindow);
        
        ShowSettingsFileCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ShowSettingsFile);
        
        ShowKeybindingsFileCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ShowKeybindingsFile);

        #endregion Settings commands
    }

    public MainViewModel()
    {
        // Only use for unit test
    }

    #region Commands

    public ReactiveCommand<Unit, Unit>? ExitCommand { get; }
    public ReactiveCommand<Unit, Unit>? MinimizeCommand { get; }
    public ReactiveCommand<Unit, Unit>? MaximizeCommand { get; }
    public ReactiveCommand<Unit, Unit>? RestoreCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleFullscreenCommand { get; }
    public ReactiveCommand<Unit, Unit>? CloseMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleFileMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleImageMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleSettingsMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleToolsMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? NewWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleLoopingCommand { get; }
    public ReactiveCommand<Unit, Unit>? RotateLeftCommand { get; }
    public ReactiveCommand<Unit, Unit>? RotateLeftButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? RotateRightCommand { get; }
    public ReactiveCommand<Unit, Unit>? RotateRightButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? RotateRightWindowBorderButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? FlipCommand { get; }
    public ReactiveCommand<Unit, Unit>? StretchCommand { get; }
    public ReactiveCommand<Unit, Unit>? CropCommand { get; }
    public ReactiveCommand<Unit, Unit>? ChangeAutoFitCommand { get; }
    public ReactiveCommand<Unit, Unit>? ChangeTopMostCommand { get; }
    public ReactiveCommand<Unit, Unit>? ChangeCtrlZoomCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleUICommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleOpeningInSameWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ChangeBackgroundCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleBottomNavBarCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleBottomGalleryShownInHiddenUICommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleFadeInButtonsOnHoverCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleTaskbarProgressCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowExifWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowAboutWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowSettingsWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowKeybindingsWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowBatchResizeWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowSingleImageResizeWindowCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowEffectsWindowCommand { get; }

    public ReactiveCommand<Unit, Unit>? OptimizeImageCommand { get; }

    public ReactiveCommand<Unit, Unit>? SortFilesByNameCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesBySizeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByExtensionCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByCreationTimeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByLastAccessTimeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesRandomlyCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesAscendingCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesDescendingCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleScrollCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleSubdirectoriesCommand { get; }

    public ReactiveCommand<Unit, Unit>? ResetSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit>? ShowSideBySideCommand { get; }

    public ReactiveCommand<Unit, Unit>? RestartCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ShowSettingsFileCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ShowKeybindingsFileCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ToggleConstrainBackgroundColorCommand { get; }

    #endregion Commands

    #region Fields
    
    #region Sorting Order

    public SortFilesBy SortOrder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsAscending
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Sorting Order

    #region Booleans

    public bool ShouldCropBeEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShouldOptimizeImageBeEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }



    public IImage? ChangeCtrlZoomImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsLoading
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUIShown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsTopToolbarShown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsBottomToolbarShown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsShowingTaskbarProgress
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFullscreen
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            ShouldRestore = IsFullscreen || IsMaximized;
            ShouldMaximizeBeShown = !IsFullscreen && !IsMaximized;
        }
    }

    public bool IsMaximized
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            ShouldRestore = IsFullscreen || IsMaximized;
            ShouldMaximizeBeShown = !IsFullscreen && !IsMaximized;
        }
    }

    public bool ShouldRestore
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShouldMaximizeBeShown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;







    public bool IsEditableTitlebarOpen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }



    public bool IsSingleImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Booleans
    
    public ImageViewer? ImageViewer;

    public int GetIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double TitleMaxWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }



    public ScrollBarVisibility ToggleScrollBarVisibility
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }



    #endregion Fields
}