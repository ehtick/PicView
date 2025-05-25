using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.Converters;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.LockScreen;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.Wallpaper;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHandling;
using PicView.Core.Gallery;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using ReactiveUI;
using ImageViewer = PicView.Avalonia.Views.ImageViewer;

namespace PicView.Avalonia.ViewModels;

public class MainViewModel : ReactiveObject
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    public TranslationViewModel Translation { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public ImageCropperViewModel? Crop { get; set; }
    public PicViewerModel PicViewer { get; } = new();
    
    public ExifViewModel Exif { get; } = new();
    
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

        #region Navigation Commands

        NextCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.Next); });

        NextButtonCommand = FunctionsHelper.CreateReactiveCommand(() => { UIHelper.NextButtonNavigation(this); });

        NextArrowButtonCommand = FunctionsHelper.CreateReactiveCommand(() => { UIHelper.NextArrowButtonNavigation(this); });

        NextFolderCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.NextFolder); });

        PreviousCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.Prev); });

        PreviousButtonCommand = FunctionsHelper.CreateReactiveCommand(() => { UIHelper.PreviousButtonNavigation(this); });

        PreviousArrowButtonCommand = FunctionsHelper.CreateReactiveCommand(() => { UIHelper.PreviousArrowButtonNavigation(this); });

        PreviousFolderCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.PrevFolder); });

        Skip10Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Next10);

        Skip100Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Next100);

        Prev10Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Prev10);

        Prev100Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Prev100);

        FirstCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.First);

        LastCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Last);

        ReloadCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Reload);

        #endregion Navigation Commands

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
            await Rotation.RotateLeft(this, Rotation.RotationButton.RotateLeftButton);
        });

        RotateRightCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.RotateRight);
        RotateRightButtonCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await Rotation.RotateRight(this, Rotation.RotationButton.RotateRightButton);
        });

        RotateRightWindowBorderButtonCommand = FunctionsHelper.CreateReactiveCommand(async () =>
        {
            await Rotation.RotateRight(this, Rotation.RotationButton.WindowBorderButton);
        });

        FlipCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Flip);

        StretchCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Stretch);

        CropCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Crop);

        ToggleScrollCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleScroll);

        OptimizeImageCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.OptimizeImage);

        ChangeBackgroundCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ChangeBackground);

        ShowSideBySideCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SideBySide);

        #endregion Image commands

        #region File commands

        OpenFileCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.Open); });

        OpenLastFileCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.OpenLastFile); });

        SaveFileCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Save);

        SaveFileAsCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.SaveAs);

        CopyFileCommand = FunctionsHelper.CreateReactiveCommand<string>(CopyFileTask);

        CopyFilePathCommand = FunctionsHelper.CreateReactiveCommand<string>(CopyFilePathTask);

        FilePropertiesCommand = FunctionsHelper.CreateReactiveCommand<string>(ShowFilePropertiesTask);

        CopyImageCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.CopyImage);

        CopyBase64Command = FunctionsHelper.CreateReactiveCommand<string>(CopyBase64Task);

        CutCommand = FunctionsHelper.CreateReactiveCommand<string>(CutFileTask);

        PasteCommand = FunctionsHelper.CreateReactiveCommand(() => { Task.Run(FunctionsMapper.Paste); });

        OpenWithCommand = FunctionsHelper.CreateReactiveCommand<string>(OpenWithTask);

        RenameCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Rename);

        ResizeCommand = FunctionsHelper.CreateReactiveCommand<int>(ResizeImageByPercentage);
        ConvertCommand = FunctionsHelper.CreateReactiveCommand<int>(ConvertFileExtension);

        DuplicateFileCommand = FunctionsHelper.CreateReactiveCommand<string>(DuplicateFileTask);

        PrintCommand = FunctionsHelper.CreateReactiveCommand<string>(PrintTask);

        DeleteFileCommand = FunctionsHelper.CreateReactiveCommand<string>(DeleteFileTask);

        RecycleFileCommand = FunctionsHelper.CreateReactiveCommand<string>(RecycleFileTask);

        LocateOnDiskCommand = FunctionsHelper.CreateReactiveCommand<string>(LocateOnDiskTask);

        SetAsWallpaperCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsWallpaperTask);
        SetAsWallpaperTiledCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsWallpaperTiledTask);
        SetAsWallpaperStretchedCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsWallpaperStretchedTask);
        SetAsWallpaperCenteredCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsWallpaperCenteredTask);
        SetAsWallpaperFilledCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsWallpaperFilledTask);

        SetAsLockScreenCommand = FunctionsHelper.CreateReactiveCommand<string>(SetAsLockScreenTask);

        #endregion File commands

        #region EXIF commands

        SetExifRating0Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set0Star);
        SetExifRating1Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set1Star);
        SetExifRating2Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set2Star);
        SetExifRating3Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set3Star);
        SetExifRating4Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set4Star);
        SetExifRating5Command = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.Set5Star);

        OpenGoogleLinkCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.OpenGoogleMaps);
        OpenBingLinkCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.OpenBingMaps);

        #endregion EXIF commands

        #region Gallery Commands

        ToggleGalleryCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleGallery);

        ToggleBottomGalleryCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.OpenCloseBottomGallery);

        CloseGalleryCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.CloseGallery);

        GalleryItemStretchCommand = FunctionsHelper.CreateReactiveCommand<string>(SetGalleryItemStretch);

        #endregion Gallery Commands

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

        ColorPickerCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ColorPicker);
        SlideshowCommand = FunctionsHelper.CreateReactiveCommand<int>(StartSlideShowTask);

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

        ToggleUsingTouchpadCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleUsingTouchpad);
        
        ToggleOpeningInSameWindowCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ToggleOpeningInSameWindow);
        
        ShowSettingsFileCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ShowSettingsFile);
        
        ShowKeybindingsFileCommand = FunctionsHelper.CreateReactiveCommand(FunctionsMapper.ShowKeybindingsFile);

        #endregion Settings commands
    }

    public MainViewModel()
    {
        // Only use for unit test
    }

    #region Gallery

    public Thickness GalleryMargin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsBottomGalleryShown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsBottomGalleryShownInHiddenUI
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public GalleryMode GalleryMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = GalleryMode.Closed;

    public Stretch GalleryStretch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int SelectedGalleryItemIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public VerticalAlignment GalleryVerticalAlignment
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = VerticalAlignment.Bottom;

    public Orientation GalleryOrientation
    {
        set => this.RaiseAndSetIfChanged(ref field, value);
        get;
    }

    public bool IsFullGalleryOpen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double GalleryWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double GalleryHeight
    {
        get
        {
            if (!Settings.Gallery.IsBottomGalleryShown || IsSingleImage || Slideshow.IsRunning)
            {
                return 0;
            }

            if (Settings.WindowProperties.Fullscreen)
            {
                return Settings.Gallery.IsBottomGalleryShown
                    ? GetBottomGalleryItemHeight + (SizeDefaults.ScrollbarSize - 1)
                    : 0;
            }

            if (!Settings.Gallery.ShowBottomGalleryInHiddenUI && !IsUIShown)
            {
                return 0;
            }

            return GetBottomGalleryItemHeight + (SizeDefaults.ScrollbarSize - 1);
        }
    }

    public double GetGalleryItemWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = double.NaN;

    public double GetGalleryItemHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double GetFullGalleryItemHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double GetBottomGalleryItemHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double MaxFullGalleryItemHeight
    {
        get => GalleryDefaults.MaxFullGalleryItemHeight;
    }

    public double MinFullGalleryItemHeight
    {
        get => GalleryDefaults.MinFullGalleryItemHeight;
    }

    public double MaxBottomGalleryItemHeight
    {
        get => GalleryDefaults.MaxBottomGalleryItemHeight;
    }

    public double MinBottomGalleryItemHeight
    {
        get => GalleryDefaults.MinBottomGalleryItemHeight;
    }

    public Thickness GalleryItemMargin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #region Gallery Stretch IsChecked

    public bool IsUniformBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUniformFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUniformMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUniformToFillBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUniformToFillFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUniformToFillMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsNoneBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsNoneFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsNoneMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsSquareBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsSquareFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsSquareMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillSquareBottomChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillSquareFullChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFillSquareMenuChecked
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion

    #endregion Gallery

    #region Commands

    public ReactiveCommand<Unit, Unit>? ExitCommand { get; }
    public ReactiveCommand<Unit, Unit>? MinimizeCommand { get; }
    public ReactiveCommand<Unit, Unit>? MaximizeCommand { get; }

    public ReactiveCommand<Unit, Unit>? RestoreCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleFullscreenCommand { get; }
    public ReactiveCommand<Unit, Unit>? NextCommand { get; }
    public ReactiveCommand<Unit, Unit>? NextButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? NextArrowButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? PreviousCommand { get; }
    public ReactiveCommand<Unit, Unit>? PreviousButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? PreviousArrowButtonCommand { get; }
    public ReactiveCommand<Unit, Unit>? NextFolderCommand { get; }
    public ReactiveCommand<Unit, Unit>? PreviousFolderCommand { get; }
    public ReactiveCommand<Unit, Unit>? FirstCommand { get; }
    public ReactiveCommand<Unit, Unit>? LastCommand { get; }
    public ReactiveCommand<Unit, Unit>? Skip10Command { get; }
    public ReactiveCommand<Unit, Unit>? Prev10Command { get; }
    public ReactiveCommand<Unit, Unit>? Skip100Command { get; }
    public ReactiveCommand<Unit, Unit>? Prev100Command { get; }
    public ReactiveCommand<Unit, Unit>? OpenFileCommand { get; }
    public ReactiveCommand<Unit, Unit>? SaveFileCommand { get; }
    public ReactiveCommand<Unit, Unit>? SaveFileAsCommand { get; }
    public ReactiveCommand<Unit, Unit>? OpenLastFileCommand { get; }
    public ReactiveCommand<Unit, Unit>? PasteCommand { get; }
    public ReactiveCommand<string, Unit>? CopyFileCommand { get; }
    public ReactiveCommand<string, Unit>? CopyBase64Command { get; }
    public ReactiveCommand<string, Unit>? CopyFilePathCommand { get; }
    public ReactiveCommand<string, Unit>? FilePropertiesCommand { get; }
    public ReactiveCommand<Unit, Unit>? CopyImageCommand { get; }
    public ReactiveCommand<string, Unit>? CutCommand { get; }
    public ReactiveCommand<Unit, Unit>? ReloadCommand { get; }
    public ReactiveCommand<string, Unit>? PrintCommand { get; }
    public ReactiveCommand<string, Unit>? DeleteFileCommand { get; }
    public ReactiveCommand<string, Unit>? RecycleFileCommand { get; }
    public ReactiveCommand<Unit, Unit>? CloseMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleFileMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleImageMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleSettingsMenuCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleToolsMenuCommand { get; }
    public ReactiveCommand<string, Unit>? LocateOnDiskCommand { get; }
    public ReactiveCommand<string, Unit>? OpenWithCommand { get; }
    public ReactiveCommand<Unit, Unit>? RenameCommand { get; }
    public ReactiveCommand<Unit, Unit>? NewWindowCommand { get; }
    public ReactiveCommand<string, Unit>? DuplicateFileCommand { get; }
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
    public ReactiveCommand<Unit, Unit>? ToggleUsingTouchpadCommand { get; }
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
    public ReactiveCommand<Unit, Unit>? SetExifRating0Command { get; }
    public ReactiveCommand<Unit, Unit>? SetExifRating1Command { get; }
    public ReactiveCommand<Unit, Unit>? SetExifRating2Command { get; }
    public ReactiveCommand<Unit, Unit>? SetExifRating3Command { get; }
    public ReactiveCommand<Unit, Unit>? SetExifRating4Command { get; }
    public ReactiveCommand<Unit, Unit>? SetExifRating5Command { get; }
    public ReactiveCommand<Unit, Unit>? OpenGoogleLinkCommand { get; }
    public ReactiveCommand<Unit, Unit>? OpenBingLinkCommand { get; }

    public ReactiveCommand<Unit, Unit>? OptimizeImageCommand { get; }
    public ReactiveCommand<int, Unit>? ResizeCommand { get; }
    public ReactiveCommand<int, Unit>? ConvertCommand { get; }

    public ReactiveCommand<Unit, Unit>? SortFilesByNameCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesBySizeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByExtensionCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByCreationTimeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesByLastAccessTimeCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesRandomlyCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesAscendingCommand { get; }
    public ReactiveCommand<Unit, Unit>? SortFilesDescendingCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleGalleryCommand { get; }
    public ReactiveCommand<Unit, Unit>? ToggleBottomGalleryCommand { get; }
    public ReactiveCommand<Unit, Unit>? CloseGalleryCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleScrollCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleSubdirectoriesCommand { get; }

    public ReactiveCommand<Unit, Unit>? ColorPickerCommand { get; }

    public ReactiveCommand<int, Unit>? SlideshowCommand { get; }

    public ReactiveCommand<string, Unit>? SetAsWallpaperCommand { get; }
    public ReactiveCommand<string, Unit>? SetAsWallpaperFilledCommand { get; }
    public ReactiveCommand<string, Unit>? SetAsWallpaperStretchedCommand { get; }
    public ReactiveCommand<string, Unit>? SetAsWallpaperTiledCommand { get; }
    public ReactiveCommand<string, Unit>? SetAsWallpaperCenteredCommand { get; }

    public ReactiveCommand<string, Unit>? SetAsLockScreenCommand { get; }

    public ReactiveCommand<string, Unit>? GalleryItemStretchCommand { get; }

    public ReactiveCommand<Unit, Unit>? ResetSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit>? ShowSideBySideCommand { get; }

    public ReactiveCommand<Unit, Unit>? RestartCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ShowSettingsFileCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ShowKeybindingsFileCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ToggleConstrainBackgroundColorCommand { get; }

    #endregion Commands

    #region Fields
    
    #region Sorting Order

    public FileListHelper.SortFilesBy SortOrder
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

    public bool IsAvoidingZoomingOut
    {
        get;
        set
        {
            Settings.Zoom.AvoidZoomingOut = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
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

    public bool IsTopMost
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsConstrainingBackgroundColor
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsIncludingSubdirectories
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsScrollingEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsStretched
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.ImageScaling.StretchImage = value;
            WindowResizing.SetSize(this);
        }
    }

    public bool IsLooping
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsAutoFit
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsStayingCentered
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.WindowProperties.KeepCentered = value;
        }
    }

    public bool IsOpeningInSameWindow
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.UIProperties.OpenInSameWindow = value;
        }
    }

    public bool IsShowingConfirmationOnEsc
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.UIProperties.ShowConfirmationOnEsc = value;
        }
    }

    public bool IsEditableTitlebarOpen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsUsingTouchpad
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.Zoom.IsUsingTouchPad = value;
        }
    }

    public bool IsSingleImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Booleans
    
    public Brush? ImageBackground
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public Brush? ConstrainedImageBackground
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public Thickness RightControlOffSetMargin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Thickness TopScreenMargin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Thickness BottomScreenMargin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public CornerRadius BottomCornerRadius
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int BackgroundChoice
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double WindowMinSize
    {
        get { return SizeDefaults.WindowMinSize; }
    }

    public double TitlebarHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double BottombarHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public UserControl? CurrentView
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ImageViewer? ImageViewer;

    public uint EXIFRating
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int GetIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double GetSlideshowSpeed
    {
        get;
        set
        {
            var roundedValue = Math.Round(value, 2);
            this.RaiseAndSetIfChanged(ref field, roundedValue);
            Settings.UIProperties.SlideShowTimer = roundedValue;
        }
    }

    public double GetNavSpeed
    {
        get => Math.Round(field, 2);
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.UIProperties.NavSpeed = value;
        }
    }

    public double GetZoomSpeed
    {
        get;
        set
        {
            var roundedValue = Math.Round(value, 2);
            this.RaiseAndSetIfChanged(ref field, roundedValue);
            Settings.Zoom.ZoomSpeed = roundedValue;
        }
    }
    
    #region Window Properties

    public SizeToContent SizeToContent
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CanResize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Window Properties

    #region Size

    public double TitleMaxWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Size

    #region Zoom

    public double RotationAngle
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double ZoomValue
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ScrollBarVisibility ToggleScrollBarVisibility
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Zoom

    #region Menus

    public bool IsFileMenuVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsImageMenuVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsSettingsMenuVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsToolsMenuVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #endregion Menus

    #endregion Fields

    #region Methods

    #region Tasks

    private async Task ResizeImageByPercentage(int percentage) =>
        await ConversionHelper.ResizeImageByPercentage(percentage, this).ConfigureAwait(false);

    private async Task ConvertFileExtension(int index) =>
        await ConversionHelper.ConvertFileExtension(index, this).ConfigureAwait(false);

    private async Task CopyFileTask(string path) => 
        await ClipboardFileOperations.CopyFileToClipboard(path, this).ConfigureAwait(false);

    private static async Task CopyFilePathTask(string path) => 
        await ClipboardTextOperations.CopyTextToClipboard(path).ConfigureAwait(false);

    private async Task CopyBase64Task(string path) =>
        await ClipboardImageOperations.CopyBase64ToClipboard(path, this).ConfigureAwait(false);

    private async Task CutFileTask(string path) =>
        await ClipboardFileOperations.CutFile(path, this).ConfigureAwait(false);

    private async Task DeleteFileTask(string path) =>
        await Task.Run(() => FileManager.DeleteFileWithOptionalDialog(false, path, PlatformService)).ConfigureAwait(false);

    private async Task RecycleFileTask(string path) =>
        await Task.Run(() => FileManager.DeleteFileWithOptionalDialog(true, path, PlatformService)).ConfigureAwait(false);

    private async Task DuplicateFileTask(string path) =>
        await ClipboardFileOperations.Duplicate(path, this).ConfigureAwait(false);

    private async Task ShowFilePropertiesTask(string path) =>
        await FileManager.ShowFileProperties(path, this).ConfigureAwait(false);

    private async Task PrintTask(string path) =>
        await FileManager.Print(path, this).ConfigureAwait(false);

    private async Task OpenWithTask(string path) => 
        await FileManager.OpenWith(path, this).ConfigureAwait(false);

    private async Task LocateOnDiskTask(string path) =>
        await FileManager.LocateOnDisk(path, this).ConfigureAwait(false);

    private async Task SetAsWallpaperTask(string path) =>
        await SetAsWallpaperTask(path, WallpaperStyle.Fit).ConfigureAwait(false);

    private async Task SetAsWallpaperFilledTask(string path) =>
        await SetAsWallpaperTask(path, WallpaperStyle.Fill).ConfigureAwait(false);

    private async Task SetAsWallpaperTiledTask(string path) =>
        await SetAsWallpaperTask(path, WallpaperStyle.Tile).ConfigureAwait(false);

    private async Task SetAsWallpaperStretchedTask(string path) =>
        await SetAsWallpaperTask(path, WallpaperStyle.Stretch).ConfigureAwait(false);

    private async Task SetAsWallpaperCenteredTask(string path) =>
        await SetAsWallpaperTask(path, WallpaperStyle.Center).ConfigureAwait(false);

    private async Task SetAsWallpaperTask(string path, WallpaperStyle style) =>
        await WallpaperManager.SetAsWallpaper(path, style, this).ConfigureAwait(false);

    private async Task SetAsLockScreenTask(string path) =>
        await LockScreenHelper.SetAsLockScreenTask(path, this).ConfigureAwait(false);

    private void SetGalleryItemStretch(string value) => GalleryHelper.SetGalleryItemStretch(value, this);

    public async Task StartSlideShowTask(int milliseconds) =>
        await Slideshow.StartSlideshow(this, milliseconds);
    
    #endregion

    #endregion Methods
}