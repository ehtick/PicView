using System.Reactive;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PicView.Avalonia.Functions;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
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
    public WindowViewModel Window { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public ImageCropperViewModel? Crop { get; set; }
    public NavigationViewModel Navigation { get; } = new();
    public FileSortingViewModel Sorting { get; } = new();
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

        #endregion Settings commands
    }

    public MainViewModel()
    {
        // Only use for unit test
    }

    #region Commands
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

    public ReactiveCommand<Unit, Unit>? OptimizeImageCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleScrollCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleSubdirectoriesCommand { get; }

    public ReactiveCommand<Unit, Unit>? ResetSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit>? ShowSideBySideCommand { get; }

    public ReactiveCommand<Unit, Unit>? RestartCommand { get; }
    
    public ReactiveCommand<Unit, Unit>? ToggleConstrainBackgroundColorCommand { get; }

    #endregion Commands

    #region Fields

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



    public bool IsShowingTaskbarProgress
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





    public ScrollBarVisibility ToggleScrollBarVisibility
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }



    #endregion Fields
}