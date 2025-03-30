using PicView.Core.Extensions;
using PicView.Core.Localization;
using ReactiveUI;

namespace PicView.Core.ViewModels;

public class TranslationViewModel : ReactiveObject
{
    public void UpdateLanguage()
    {
        File = TranslationManager.Translation.File;
        SelectFile = TranslationManager.Translation.OpenFileDialog;
        OpenLastFile = TranslationManager.Translation.OpenLastFile;
        Paste = TranslationManager.Translation.FilePaste;
        Copy = TranslationManager.Translation.Copy;
        Reload = TranslationManager.Translation.Reload;
        Print = TranslationManager.Translation.Print;
        DeleteFile = TranslationManager.Translation.DeleteFile;
        PermanentlyDelete = TranslationManager.Translation.PermanentlyDelete;
        Save = TranslationManager.Translation.Save;
        CopyFile = TranslationManager.Translation.CopyFile;
        NewWindow = TranslationManager.Translation.NewWindow;
        Close = TranslationManager.Translation.Close;
        Open = TranslationManager.Translation.Open;
        OpenFileDialog = TranslationManager.Translation.OpenFileDialog;
        ShowInFolder = TranslationManager.Translation.ShowInFolder;
        OpenWith = TranslationManager.Translation.OpenWith;
        RenameFile = TranslationManager.Translation.RenameFile;
        DuplicateFile = TranslationManager.Translation.DuplicateFile;
        RotateLeft = TranslationManager.Translation.RotateLeft;
        RotateRight = TranslationManager.Translation.RotateRight;
        Flip = TranslationManager.Translation.Flip;
        UnFlip = TranslationManager.Translation.Unflip;
        ShowBottomGallery = TranslationManager.Translation.ShowBottomGallery;
        HideBottomGallery = TranslationManager.Translation.HideBottomGallery;
        AutoFitWindow = TranslationManager.Translation.AutoFitWindow;
        Stretch = TranslationManager.Translation.Stretch;
        Crop = TranslationManager.Translation.Crop;
        ResizeImage = TranslationManager.Translation.ResizeImage;
        GoToImageAtSpecifiedIndex = TranslationManager.Translation.GoToImageAtSpecifiedIndex;
        ToggleScroll = TranslationManager.Translation.ToggleScroll;
        ScrollEnabled = TranslationManager.Translation.ScrollingEnabled;
        ScrollDisabled = TranslationManager.Translation.ScrollingDisabled;
        ScrollDirection = TranslationManager.Translation.ScrollDirection;
        Reverse = TranslationManager.Translation.Reverse;
        Forward = TranslationManager.Translation.Forward;
        Slideshow = TranslationManager.Translation.Slideshow;
        Settings = TranslationManager.Translation.Settings;
        AboutWindow = TranslationManager.Translation.InfoWindow;
        ImageInfo = TranslationManager.Translation.ImageInfo;
        About = TranslationManager.Translation.About;
        ShowAllSettingsWindow = TranslationManager.Translation.ShowAllSettingsWindow;
        StayTopMost = TranslationManager.Translation.StayTopMost;
        SearchSubdirectory = TranslationManager.Translation.SearchSubdirectory;
        ToggleLooping = TranslationManager.Translation.ToggleLooping;
        ApplicationShortcuts = TranslationManager.Translation.ApplicationShortcuts;
        BatchResize = TranslationManager.Translation.BatchResize;
        Effects = TranslationManager.Translation.Effects;
        EffectsTooltip = TranslationManager.Translation.EffectsTooltip;
        FileProperties = TranslationManager.Translation.FileProperties;
        OptimizeImage = TranslationManager.Translation.OptimizeImage;
        ImageInfo = TranslationManager.Translation.ImageInfo;
        FileName = TranslationManager.Translation.FileName;
        FileSize = TranslationManager.Translation.FileSize;
        Folder = TranslationManager.Translation.Folder;
        FullPath = TranslationManager.Translation.FullPath;
        Created = TranslationManager.Translation.Created;
        Modified = TranslationManager.Translation.Modified;
        LastAccessTime = TranslationManager.Translation.LastAccessTime;
        ConvertTo = TranslationManager.Translation.ConvertTo;
        NoConversion = TranslationManager.Translation.NoConversion;
        Resize = TranslationManager.Translation.Resize;
        NoResize = TranslationManager.Translation.NoResize;
        Apply = TranslationManager.Translation.Apply;
        Cancel = TranslationManager.Translation.Cancel;
        BitDepth = TranslationManager.Translation.BitDepth;
        ReadAbleAspectRatio = TranslationManager.Translation.AspectRatio;
        Width = TranslationManager.Translation.Width;
        Height = TranslationManager.Translation.Height;
        SizeMp = TranslationManager.Translation.SizeMp;
        Resolution = TranslationManager.Translation.Resolution;
        PrintSizeIn = TranslationManager.Translation.PrintSizeIn;
        PrintSizeCm = TranslationManager.Translation.PrintSizeCm;
        Centimeters = TranslationManager.Translation.Centimeters;
        Inches = TranslationManager.Translation.Inches;
        SizeTooltip = TranslationManager.Translation.SizeTooltip;
        Latitude = TranslationManager.Translation.Latitude;
        Longitude = TranslationManager.Translation.Longitude;
        Altitude = TranslationManager.Translation.Altitude;
        Authors = TranslationManager.Translation.Authors;
        DateTaken = TranslationManager.Translation.DateTaken;
        Copyright = TranslationManager.Translation.Copyright;
        ResolutionUnit = TranslationManager.Translation.ResolutionUnit;
        ColorRepresentation = TranslationManager.Translation.ColorRepresentation;
        CompressedBitsPixel = TranslationManager.Translation.CompressedBitsPixel;
        Compression = TranslationManager.Translation.Compression;
        ExposureTime = TranslationManager.Translation.ExposureTime;
        Title = TranslationManager.Translation.Title;
        Subject = TranslationManager.Translation.Subject;
        Software = TranslationManager.Translation.Software;
        CameraMaker = TranslationManager.Translation.CameraMaker;
        CameraModel = TranslationManager.Translation.CameraModel;
        FocalLength = TranslationManager.Translation.FocalLength;
        Fnumber = TranslationManager.Translation.FNumber;
        Fstop = TranslationManager.Translation.Fstop;
        MaxAperture = TranslationManager.Translation.MaxAperture;
        ExposureBias = TranslationManager.Translation.ExposureBias;
        ExposureProgram = TranslationManager.Translation.ExposureProgram;
        DigitalZoom = TranslationManager.Translation.DigitalZoom;
        ISOSpeed = TranslationManager.Translation.ISOSpeed;
        FocalLength35mm = TranslationManager.Translation.FocalLength35mm;
        MeteringMode = TranslationManager.Translation.MeteringMode;
        Contrast = TranslationManager.Translation.Contrast;
        Saturation = TranslationManager.Translation.Saturation;
        Sharpness = TranslationManager.Translation.Sharpness;
        WhiteBalance = TranslationManager.Translation.WhiteBalance;
        FlashEnergy = TranslationManager.Translation.FlashEnergy;
        FlashMode = TranslationManager.Translation.FlashMode;
        LightSource = TranslationManager.Translation.LightSource;
        Brightness = TranslationManager.Translation.Brightness;
        PhotometricInterpretation = TranslationManager.Translation.PhotometricInterpretation;
        Orientation = TranslationManager.Translation.Orientation;
        ExifVersion = TranslationManager.Translation.ExifVersion;
        LensMaker = TranslationManager.Translation.LensMaker;
        LensModel = TranslationManager.Translation.LensModel;
        SortFilesBy = TranslationManager.Translation.SortFilesBy;
        FileExtension = TranslationManager.Translation.FileExtension;
        CreationTime = TranslationManager.Translation.CreationTime;
        Random = TranslationManager.Translation.Random;
        Ascending = TranslationManager.Translation.Ascending;
        Descending = TranslationManager.Translation.Descending;
        RecentFiles = TranslationManager.Translation.RecentFiles;
        SetAsWallpaper = TranslationManager.Translation.SetAsWallpaper;
        SetAsLockScreenImage = TranslationManager.Translation.SetAsLockScreenImage;
        Image = TranslationManager.Translation.Image;
        CopyImage = TranslationManager.Translation.CopyImage;
        FileCopyPath = TranslationManager.Translation.FileCopyPath;
        FileCut = TranslationManager.Translation.Cut;
        CtrlToZoom = TranslationManager.Translation.CtrlToZoom;
        ScrollToZoom = TranslationManager.Translation.ScrollToZoom;
        GeneralSettings = TranslationManager.Translation.GeneralSettings;
        Appearance = TranslationManager.Translation.Appearance;
        Language = TranslationManager.Translation.Language;
        MouseWheel = TranslationManager.Translation.MouseWheel;
        MiscSettings = TranslationManager.Translation.MiscSettings;
        StayCentered = TranslationManager.Translation.StayCentered;
        ShowFileSavingDialog = TranslationManager.Translation.ShowFileSavingDialog;
        OpenInSameWindow = TranslationManager.Translation.OpenInSameWindow;
        ApplicationStartup = TranslationManager.Translation.ApplicationStartup;
        None = TranslationManager.Translation.None;
        AdjustTimingForSlideshow = TranslationManager.Translation.AdjustTimingForSlideshow;
        AdjustTimingForZoom = TranslationManager.Translation.AdjustTimingForZoom;
        AdjustNavSpeed = TranslationManager.Translation.AdjustNavSpeed;
        SecAbbreviation = TranslationManager.Translation.SecAbbreviation;
        ResetButtonText = TranslationManager.Translation.ResetButtonText;
        ShowBottomToolbar = TranslationManager.Translation.ShowBottomToolbar;
        ShowBottomGalleryWhenUiIsHidden = TranslationManager.Translation.ShowBottomGalleryWhenUiIsHidden;
        ChangeKeybindingTooltip = TranslationManager.Translation.ChangeKeybindingTooltip;
        ToggleTaskbarProgress = TranslationManager.Translation.ToggleTaskbarProgress;
        ChangeKeybindingText = TranslationManager.Translation.ChangeKeybindingText;
        Navigation = TranslationManager.Translation.Navigation;
        NextImage = TranslationManager.Translation.NextImage;
        PrevImage = TranslationManager.Translation.PrevImage;
        LastImage = TranslationManager.Translation.LastImage;
        FirstImage = TranslationManager.Translation.FirstImage;
        NextFolder = TranslationManager.Translation.NextFolder;
        PrevFolder = TranslationManager.Translation.PrevFolder;
        SelectGalleryThumb = TranslationManager.Translation.SelectGalleryThumb;
        ScrollAndRotate = TranslationManager.Translation.ScrollAndRotate;
        ScrollUp = TranslationManager.Translation.ScrollUp;
        ScrollDown = TranslationManager.Translation.ScrollDown;
        ScrollToTop = TranslationManager.Translation.ScrollToTop;
        ScrollToBottom = TranslationManager.Translation.ScrollToBottom;
        Zoom = TranslationManager.Translation.Zoom;
        ZoomIn = TranslationManager.Translation.ZoomIn;
        ZoomOut = TranslationManager.Translation.ZoomOut;
        Pan = TranslationManager.Translation.Pan;
        ResetZoom = TranslationManager.Translation.ResetZoom;
        ImageControl = TranslationManager.Translation.ImageControl;
        ChangeBackground = TranslationManager.Translation.ChangeBackground;
        InterfaceConfiguration = TranslationManager.Translation.InterfaceConfiguration;
        FileManagement = TranslationManager.Translation.FileManagement;
        ToggleFullscreen = TranslationManager.Translation.ToggleFullscreen;
        Fullscreen = TranslationManager.Translation.Fullscreen;
        ShowImageGallery = TranslationManager.Translation.ShowImageGallery;
        WindowManagement = TranslationManager.Translation.WindowManagement;
        CenterWindow = TranslationManager.Translation.CenterWindow;
        WindowScaling = TranslationManager.Translation.WindowScaling;
        NormalWindow = TranslationManager.Translation.NormalWindow;
        SetStarRating = TranslationManager.Translation.SetStarRating;
        _1Star = TranslationManager.Translation._1Star;
        _2Star = TranslationManager.Translation._2Star;
        _3Star = TranslationManager.Translation._3Star;
        _4Star = TranslationManager.Translation._4Star;
        _5Star = TranslationManager.Translation._5Star;
        RemoveStarRating = TranslationManager.Translation.RemoveStarRating;
        Theme = TranslationManager.Translation.Theme;
        DarkTheme = TranslationManager.Translation.DarkTheme;
        LightTheme = TranslationManager.Translation.LightTheme;
        MouseDrag = TranslationManager.Translation.MouseDrag;
        DoubleClick = TranslationManager.Translation.DoubleClick;
        MoveWindow = TranslationManager.Translation.MoveWindow;
        GithubRepo = TranslationManager.Translation.GithubRepo;
        Version = TranslationManager.Translation.Version;
        ViewLicenseFile = TranslationManager.Translation.ViewLicenseFile;
        CheckForUpdates = TranslationManager.Translation.CheckForUpdates;
        Credits = TranslationManager.Translation.Credits;
        ColorPickerTool = TranslationManager.Translation.ColorPickerTool;
        ColorPickerToolTooltip = TranslationManager.Translation.ColorPickerToolTooltip;
        ExpandedGalleryItemSize = TranslationManager.Translation.ExpandedGalleryItemSize;
        BottomGalleryItemSize = TranslationManager.Translation.BottomGalleryItemSize;
        Square = TranslationManager.Translation.Square;
        Uniform = TranslationManager.Translation.Uniform;
        UniformToFill = TranslationManager.Translation.UniformToFill;
        FillSquare = TranslationManager.Translation.FillSquare;
        Fill = TranslationManager.Translation.Fill;
        GallerySettings = TranslationManager.Translation.GallerySettings;
        GalleryThumbnailStretch = TranslationManager.Translation.GalleryThumbnailStretch;
        BottomGalleryThumbnailStretch = TranslationManager.Translation.BottomGalleryThumbnailStretch;
        RestoreDown = TranslationManager.Translation.RestoreDown;
        SideBySide = TranslationManager.Translation.SideBySide;
        SideBySideTooltip = TranslationManager.Translation.SideBySideTooltip;
        HighlightColor = TranslationManager.Translation.HighlightColor;
        AllowZoomOut = TranslationManager.Translation.AllowZoomOut;
        GlassTheme = TranslationManager.Translation.GlassTheme;
        ChangingThemeRequiresRestart = TranslationManager.Translation.ChangingThemeRequiresRestart;
        ShowUI = TranslationManager.Translation.ShowUI;
        HideUI = TranslationManager.Translation.HideUI;
        HideBottomToolbar = TranslationManager.Translation.HideBottomToolbar;
        Center = TranslationManager.Translation.Center;
        Tile = TranslationManager.Translation.Tile;
        Fit = TranslationManager.Translation.Fit;
        Pixels = TranslationManager.Translation.Pixels;
        Percentage = TranslationManager.Translation.Percentage;
        Quality = TranslationManager.Translation.Quality;
        SaveAs = TranslationManager.Translation.SaveAs;
        Reset = TranslationManager.Translation.Reset;
        AdvanceBy10Images = TranslationManager.Translation.AdvanceBy10Images;
        AdvanceBy100Images = TranslationManager.Translation.AdvanceBy100Images;
        GoBackBy10Images = TranslationManager.Translation.GoBackBy10Images;
        GoBackBy100Images = TranslationManager.Translation.GoBackBy100Images;
        ShowFadeInButtonsOnHover = TranslationManager.Translation.ShowFadeInButtonsOnHover;
        DisableFadeInButtonsOnHover = TranslationManager.Translation.DisableFadeInButtonsOnHover;
        UsingTouchpad = TranslationManager.Translation.UsingTouchpad;
        UsingMouse = TranslationManager.Translation.UsingMouse;
        SourceFolder = TranslationManager.Translation.SourceFolder;
        OutputFolder = TranslationManager.Translation.OutputFolder;
        GenerateThumbnails = TranslationManager.Translation.GenerateThumbnails;
        Lossless = TranslationManager.Translation.Lossless;
        Lossy = TranslationManager.Translation.Lossy;
        Start = TranslationManager.Translation.Start;
        Thumbnail = TranslationManager.Translation.Thumbnail;
        WidthAndHeight = TranslationManager.Translation.WidthAndHeight;
        CloseWindowPrompt = TranslationManager.Translation.CloseWindowPrompt;
        ShowConfirmationOnEsc = TranslationManager.Translation.ShowConfirmationOnEsc;
        ImageAliasing = TranslationManager.Translation.ImageAliasing;
        HighQuality = TranslationManager.Translation.HighQuality;
        Lighting = TranslationManager.Translation.Lighting;
        BlackAndWhite = TranslationManager.Translation.BlackAndWhite;
        NegativeColors = TranslationManager.Translation.NegativeColors;
        Blur = TranslationManager.Translation.Blur;
        PencilSketch = TranslationManager.Translation.PencilSketch;
        OldMovie = TranslationManager.Translation.OldMovie;
        Posterize = TranslationManager.Translation.Posterize;
        ClearEffects = TranslationManager.Translation.ClearEffects;
        Solarize = TranslationManager.Translation.Solarize;
        Maximize = TranslationManager.Translation.Maximize;
        SelectAll = TranslationManager.Translation.SelectAll;
        Normal = TranslationManager.Translation.Normal;
        FileAssociations = TranslationManager.Translation.FileAssociations;
        SelectFileTypesToAssociate = TranslationManager.Translation.SelectFileTypesToAssociate;
        Filter = TranslationManager.Translation.Filter;
        UnselectAll = TranslationManager.Translation.UnselectAll;
        Unassociate = TranslationManager.Translation.Unassociate;
        ShowConfirmationDialogWhenMovingFileToRecycleBin = TranslationManager.Translation.ShowConfirmationDialogWhenMovingFileToRecycleBin;
        MoveToRecycleBin = TranslationManager.Translation.MoveToRecycleBin;
        ShowConfirmationDialogWhenPermanentlyDeletingFile = TranslationManager.Translation.ShowConfirmationDialogWhenPermanentlyDeletingFile;
        Downloading = TranslationManager.Translation.Downloading;
    }

    #region Static Translation Strings
    
    public string? Downloading
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? _1Star
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? _2Star
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? _3Star
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? _4Star
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? _5Star
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? About
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AboutWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AdjustNavSpeed
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AdjustTimingForSlideshow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AdjustTimingForZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AdvanceBy100Images
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AdvanceBy10Images
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AllowZoomOut
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Altitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Appearance
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ApplicationShortcuts
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ApplicationStartup
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Apply
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Ascending
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Authors
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AutoFitWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BatchResize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BitDepth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BlackAndWhite
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Blur
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BottomGalleryItemSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BottomGalleryThumbnailStretch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Brightness
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CameraMaker
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CameraModel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Cancel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Center
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CenterWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Centimeters
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ChangeBackground
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ChangeKeybindingText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ChangeKeybindingTooltip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ChangingThemeRequiresRestart
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CheckForUpdates
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ClearEffects
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Close
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CloseWindowPrompt
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ColorPickerTool
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ColorPickerToolTooltip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ColorRepresentation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CompressedBitsPixel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Compression
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Contrast
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ConvertTo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Copy
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CopyFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CopyImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Copyright
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Created
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CreationTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Credits
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Crop
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CtrlToZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DarkTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DateTaken
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DeleteFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Descending
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DigitalZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DirectionalBlur
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DisableFadeInButtonsOnHover
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DoubleClick
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DuplicateFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Effects
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? EffectsTooltip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExifVersion
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExpandedGalleryItemSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureBias
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureProgram
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? File
    {
        get => field.FirstCharToUpper();
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileAssociations
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileCopyPath
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileCut
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileExtension
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileManagement
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileName
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileProperties
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FileSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Fill
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FillSquare
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Filter
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FirstImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Fit
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FlashEnergy
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FlashMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Flip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Fnumber
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FocalLength
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FocalLength35mm
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Folder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Forward
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Fstop
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FullPath
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Fullscreen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GallerySettings
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GalleryThumbnailStretch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GeneralSettings
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GenerateThumbnails
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GithubRepo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GlassTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GoBackBy100Images
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GoBackBy10Images
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GoToImageAtSpecifiedIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Height
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? HideBottomGallery
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? HideBottomToolbar
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? HideUI
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? HighlightColor
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? HighQuality
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Image
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ImageAliasing
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ImageControl
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ImageInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Inches
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? InterfaceConfiguration
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ISOSpeed
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Language
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LastAccessTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LastImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Latitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LensMaker
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LensModel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Lighting
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LightSource
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LightTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Longitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LoopingDisabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LoopingEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Lossless
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Lossy
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MaxAperture
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Maximize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MeteringMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MiscSettings
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Modified
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MouseDrag
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MouseWheel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MoveToRecycleBin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MoveWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Navigation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NegativeColors
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NewWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NextFolder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NextImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NoConversion
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? None
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NoResize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Normal
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NormalWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OldMovie
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Open
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OpenFileDialog
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OpenInSameWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OpenLastFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OpenWith
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OptimizeImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Orientation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? OutputFolder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Pan
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Paste
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PencilSketch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Percentage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PermanentlyDelete
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PhotometricInterpretation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Pixels
    {
        get => field.FirstCharToUpper();
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Posterize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PrevFolder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PrevImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Print
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PrintSizeCm
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PrintSizeIn
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Quality
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Random
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ReadAbleAspectRatio
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RecentFiles
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Reload
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RemoveStarRating
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RenameFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Reset
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResetButtonText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResetZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Resize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResizeImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Resolution
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResolutionUnit
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RestoreDown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Reverse
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RotateLeft
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? RotateRight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Saturation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Save
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SaveAs
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollAndRotate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollDirection
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollDisabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollDown
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollToBottom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollToTop
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollToZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ScrollUp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SearchSubdirectory
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SecAbbreviation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SelectAll
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SelectFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SelectFileTypesToAssociate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SelectGalleryThumb
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SetAsLockScreenImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SetAsWallpaper
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SetStarRating
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Settings
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Sharpness
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowAllSettingsWindow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowBottomGallery
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowBottomGalleryWhenUiIsHidden
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowBottomToolbar
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowConfirmationDialogWhenMovingFileToRecycleBin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowConfirmationDialogWhenPermanentlyDeletingFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? ShowConfirmationOnEsc
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowFadeInButtonsOnHover
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowFileSavingDialog
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowImageGallery
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowInFolder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ShowUI
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SideBySide
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SideBySideTooltip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SizeMp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SizeTooltip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Slideshow
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Software
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Solarize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SortFilesBy
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SourceFolder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Square
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Start
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? StayCentered
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? StayTopMost
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Stretch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Subject
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Theme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Thumbnail
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Tile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Title
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ToggleFullscreen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ToggleLooping
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ToggleScroll
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ToggleTaskbarProgress
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Unassociate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? UnFlip
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Uniform
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? UniformToFill
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? UnselectAll
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? UsingMouse
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? UsingTouchpad
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Version
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ViewLicenseFile
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? WhiteBalance
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Width
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? WidthAndHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? WindowManagement
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? WindowScaling
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? Zoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ZoomIn
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ZoomOut
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    #endregion strings

    #region Dynamic Translation strings

    public string? IsCtrlToZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsFlipped
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsLooping
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsScrolling
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsShowingBottomGallery
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsShowingBottomToolbar
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsShowingFadingUIButtons
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? IsShowingUI
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? IsUsingTouchpad
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    #endregion
}