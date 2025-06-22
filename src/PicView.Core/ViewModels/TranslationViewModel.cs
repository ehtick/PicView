using PicView.Core.Extensions;
using PicView.Core.Localization;
using ReactiveUI;

namespace PicView.Core.ViewModels;

public class TranslationViewModel : ReactiveObject
{
    public void UpdateLanguage()
    {
        var t = TranslationManager.Translation;
        
        File = t.File;
        SelectFile = t.OpenFileDialog;
        OpenLastFile = t.OpenLastFile;
        Paste = t.FilePaste;
        Copy = t.Copy;
        Reload = t.Reload;
        Print = t.Print;
        DeleteFile = t.DeleteFile;
        PermanentlyDelete = t.PermanentlyDelete;
        Save = t.Save;
        CopyFile = t.CopyFile;
        NewWindow = t.NewWindow;
        Close = t.Close;
        CloseGallery = t.CloseGallery;
        Open = t.Open;
        OpenFileDialog = t.OpenFileDialog;
        ShowInFolder = t.ShowInFolder;
        OpenWith = t.OpenWith;
        RenameFile = t.RenameFile;
        DuplicateFile = t.DuplicateFile;
        RotateLeft = t.RotateLeft;
        RotateRight = t.RotateRight;
        Flip = t.Flip;
        UnFlip = t.Unflip;
        ShowBottomGallery = t.ShowBottomGallery;
        HideBottomGallery = t.HideBottomGallery;
        AutoFitWindow = t.AutoFitWindow;
        Stretch = t.Stretch;
        Crop = t.Crop;
        ResizeImage = t.ResizeImage;
        GoToImageAtSpecifiedIndex = t.GoToImageAtSpecifiedIndex;
        ToggleScroll = t.ToggleScroll;
        ScrollEnabled = t.ScrollingEnabled;
        ScrollDisabled = t.ScrollingDisabled;
        ScrollDirection = t.ScrollDirection;
        Reverse = t.Reverse;
        Forward = t.Forward;
        Slideshow = t.Slideshow;
        Settings = t.Settings;
        AboutWindow = t.InfoWindow;
        ImageInfo = t.ImageInfo;
        About = t.About;
        ShowAllSettingsWindow = t.ShowAllSettingsWindow;
        StayTopMost = t.StayTopMost;
        SearchSubdirectory = t.SearchSubdirectory;
        ToggleLooping = t.ToggleLooping;
        ApplicationShortcuts = t.ApplicationShortcuts;
        BatchResize = t.BatchResize;
        Effects = t.Effects;
        EffectsTooltip = t.EffectsTooltip;
        FileProperties = t.FileProperties;
        OptimizeImage = t.OptimizeImage;
        ImageInfo = t.ImageInfo;
        FileName = t.FileName;
        FileSize = t.FileSize;
        Folder = t.Folder;
        FullPath = t.FullPath;
        Created = t.Created;
        Modified = t.Modified;
        LastAccessTime = t.LastAccessTime;
        ConvertTo = t.ConvertTo;
        NoConversion = t.NoConversion;
        Resize = t.Resize;
        NoResize = t.NoResize;
        Apply = t.Apply;
        Cancel = t.Cancel;
        BitDepth = t.BitDepth;
        ReadAbleAspectRatio = t.AspectRatio;
        Width = t.Width;
        Height = t.Height;
        SizeMp = t.SizeMp;
        Resolution = t.Resolution;
        PrintSizeIn = t.PrintSizeIn;
        PrintSizeCm = t.PrintSizeCm;
        Centimeters = t.Centimeters;
        Inches = t.Inches;
        SizeTooltip = t.SizeTooltip;
        Latitude = t.Latitude;
        Longitude = t.Longitude;
        Altitude = t.Altitude;
        Authors = t.Authors;
        DateTaken = t.DateTaken;
        Copyright = t.Copyright;
        ResolutionUnit = t.ResolutionUnit;
        ColorRepresentation = t.ColorRepresentation;
        CompressedBitsPixel = t.CompressedBitsPixel;
        Compression = t.Compression;
        ExposureTime = t.ExposureTime;
        Title = t.Title;
        Subject = t.Subject;
        Software = t.Software;
        CameraMaker = t.CameraMaker;
        CameraModel = t.CameraModel;
        FocalLength = t.FocalLength;
        Fnumber = t.FNumber;
        Fstop = t.Fstop;
        MaxAperture = t.MaxAperture;
        ExposureBias = t.ExposureBias;
        ExposureProgram = t.ExposureProgram;
        DigitalZoom = t.DigitalZoom;
        ISOSpeed = t.ISOSpeed;
        FocalLength35mm = t.FocalLength35mm;
        MeteringMode = t.MeteringMode;
        Contrast = t.Contrast;
        Saturation = t.Saturation;
        Sharpness = t.Sharpness;
        WhiteBalance = t.WhiteBalance;
        FlashEnergy = t.FlashEnergy;
        FlashMode = t.FlashMode;
        LightSource = t.LightSource;
        Brightness = t.Brightness;
        PhotometricInterpretation = t.PhotometricInterpretation;
        Orientation = t.Orientation;
        ExifVersion = t.ExifVersion;
        LensMaker = t.LensMaker;
        LensModel = t.LensModel;
        SortFilesBy = t.SortFilesBy;
        FileExtension = t.FileExtension;
        CreationTime = t.CreationTime;
        Random = t.Random;
        Ascending = t.Ascending;
        Descending = t.Descending;
        RecentFiles = t.RecentFiles;
        SetAsWallpaper = t.SetAsWallpaper;
        SetAsLockScreenImage = t.SetAsLockScreenImage;
        Image = t.Image;
        CopyImage = t.CopyImage;
        FileCopyPath = t.FileCopyPath;
        FileCut = t.Cut;
        CtrlToZoom = t.CtrlToZoom;
        ScrollToZoom = t.ScrollToZoom;
        GeneralSettings = t.GeneralSettings;
        Appearance = t.Appearance;
        Language = t.Language;
        MouseWheel = t.MouseWheel;
        MiscSettings = t.MiscSettings;
        StayCentered = t.StayCentered;
        ShowFileSavingDialog = t.ShowFileSavingDialog;
        OpenInSameWindow = t.OpenInSameWindow;
        ApplicationStartup = t.ApplicationStartup;
        None = t.None;
        AdjustTimingForSlideshow = t.AdjustTimingForSlideshow;
        AdjustTimingForZoom = t.AdjustTimingForZoom;
        AdjustNavSpeed = t.AdjustNavSpeed;
        SecAbbreviation = t.SecAbbreviation;
        ResetButtonText = t.ResetButtonText;
        ShowBottomToolbar = t.ShowBottomToolbar;
        ShowBottomGalleryWhenUiIsHidden = t.ShowBottomGalleryWhenUiIsHidden;
        ChangeKeybindingTooltip = t.ChangeKeybindingTooltip;
        ToggleTaskbarProgress = t.ToggleTaskbarProgress;
        ChangeKeybindingText = t.ChangeKeybindingText;
        Navigation = t.Navigation;
        NextImage = t.NextImage;
        PrevImage = t.PrevImage;
        LastImage = t.LastImage;
        FirstImage = t.FirstImage;
        NextFolder = t.NextFolder;
        PrevFolder = t.PrevFolder;
        SelectGalleryThumb = t.SelectGalleryThumb;
        ScrollAndRotate = t.ScrollAndRotate;
        ScrollUp = t.ScrollUp;
        ScrollDown = t.ScrollDown;
        ScrollToTop = t.ScrollToTop;
        ScrollToBottom = t.ScrollToBottom;
        Zoom = t.Zoom;
        ZoomIn = t.ZoomIn;
        ZoomOut = t.ZoomOut;
        Pan = t.Pan;
        ResetZoom = t.ResetZoom;
        ImageControl = t.ImageControl;
        ChangeBackground = t.ChangeBackground;
        InterfaceConfiguration = t.InterfaceConfiguration;
        FileManagement = t.FileManagement;
        ToggleFullscreen = t.ToggleFullscreen;
        Fullscreen = t.Fullscreen;
        ShowImageGallery = t.ShowImageGallery;
        WindowManagement = t.WindowManagement;
        CenterWindow = t.CenterWindow;
        WindowScaling = t.WindowScaling;
        NormalWindow = t.NormalWindow;
        SetStarRating = t.SetStarRating;
        _1Star = t._1Star;
        _2Star = t._2Star;
        _3Star = t._3Star;
        _4Star = t._4Star;
        _5Star = t._5Star;
        RemoveStarRating = t.RemoveStarRating;
        Theme = t.Theme;
        DarkTheme = t.DarkTheme;
        LightTheme = t.LightTheme;
        MouseDrag = t.MouseDrag;
        DoubleClick = t.DoubleClick;
        MoveWindow = t.MoveWindow;
        GithubRepo = t.GithubRepo;
        Version = t.Version;
        ViewLicenseFile = t.ViewLicenseFile;
        CheckForUpdates = t.CheckForUpdates;
        Credits = t.Credits;
        ColorPickerTool = t.ColorPickerTool;
        ColorPickerToolTooltip = t.ColorPickerToolTooltip;
        ExpandedGalleryItemSize = t.ExpandedGalleryItemSize;
        BottomGalleryItemSize = t.BottomGalleryItemSize;
        Square = t.Square;
        Uniform = t.Uniform;
        UniformToFill = t.UniformToFill;
        FillSquare = t.FillSquare;
        Fill = t.Fill;
        GallerySettings = t.GallerySettings;
        GalleryThumbnailStretch = t.GalleryThumbnailStretch;
        BottomGalleryThumbnailStretch = t.BottomGalleryThumbnailStretch;
        RestoreDown = t.RestoreDown;
        SideBySide = t.SideBySide;
        SideBySideTooltip = t.SideBySideTooltip;
        HighlightColor = t.HighlightColor;
        AllowZoomOut = t.AllowZoomOut;
        GlassTheme = t.GlassTheme;
        ChangingThemeRequiresRestart = t.ChangingThemeRequiresRestart;
        ShowUI = t.ShowUI;
        HideUI = t.HideUI;
        HideBottomToolbar = t.HideBottomToolbar;
        Center = t.Center;
        Tile = t.Tile;
        Fit = t.Fit;
        Pixels = t.Pixels;
        Percentage = t.Percentage;
        Quality = t.Quality;
        SaveAs = t.SaveAs;
        Reset = t.Reset;
        AdvanceBy10Images = t.AdvanceBy10Images;
        AdvanceBy100Images = t.AdvanceBy100Images;
        GoBackBy10Images = t.GoBackBy10Images;
        GoBackBy100Images = t.GoBackBy100Images;
        ShowFadeInButtonsOnHover = t.ShowFadeInButtonsOnHover;
        DisableFadeInButtonsOnHover = t.DisableFadeInButtonsOnHover;
        UsingTouchpad = t.UsingTouchpad;
        UsingMouse = t.UsingMouse;
        SourceFolder = t.SourceFolder;
        OutputFolder = t.OutputFolder;
        GenerateThumbnails = t.GenerateThumbnails;
        Lossless = t.Lossless;
        Lossy = t.Lossy;
        Start = t.Start;
        Thumbnail = t.Thumbnail;
        WidthAndHeight = t.WidthAndHeight;
        CloseWindowPrompt = t.CloseWindowPrompt;
        ShowConfirmationOnEsc = t.ShowConfirmationOnEsc;
        ImageAliasing = t.ImageAliasing;
        HighQuality = t.HighQuality;
        Lighting = t.Lighting;
        BlackAndWhite = t.BlackAndWhite;
        NegativeColors = t.NegativeColors;
        Blur = t.Blur;
        PencilSketch = t.PencilSketch;
        OldMovie = t.OldMovie;
        Posterize = t.Posterize;
        ClearEffects = t.ClearEffects;
        Solarize = t.Solarize;
        Maximize = t.Maximize;
        SelectAll = t.SelectAll;
        Normal = t.Normal;
        FileAssociations = t.FileAssociations;
        SelectFileTypesToAssociate = t.SelectFileTypesToAssociate;
        Filter = t.Filter;
        UnselectAll = t.UnselectAll;
        Unassociate = t.Unassociate;
        ShowConfirmationDialogWhenMovingFileToRecycleBin = t.ShowConfirmationDialogWhenMovingFileToRecycleBin;
        MoveToRecycleBin = t.MoveToRecycleBin;
        ShowConfirmationDialogWhenPermanentlyDeletingFile = t.ShowConfirmationDialogWhenPermanentlyDeletingFile;
        Downloading = t.Downloading;
        Pinned = t.Pinned;
        Unpin = t.Unpin;
        Pin = t.Pin;
        Clear = t.Clear;
        OpenFileHistory = t.OpenFileHistory;
        ConstrainBackgroundToImage = t.ConstrainBackgroundToImage;
        Window = t.Window;
        WindowMargin = t.WindowMargin;
        Mouse = t.Mouse;
        MouseSideButtons = t.MouseSideButtons;
        NavigateFileHistory = t.NavigateFileHistory;
        NavigateBetweenDirectories = t.NavigateBetweenDirectories;
        Comment = t.Comment;
    }

    #region Static Translation Strings

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

    public string? Clear
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

    public string? CloseGallery
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

    public string? Comment
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

    public string? ConstrainBackgroundToImage
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

    public string? Downloading
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
    
    public string? Mouse
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MouseDrag
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public string? MouseSideButtons
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

    public string? NavigateBetweenDirectories
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? NavigateFileHistory
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

    public string? OpenFileHistory
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

    public string? Pin
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Pinned
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

    public string? Unpin
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

    public string? Window
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? WindowManagement
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public string? WindowMargin
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