
namespace PicView.Core.IPlatform;

public interface IFunctionsMapper
{
    Func<ValueTask>? GetFunctionByName(string functionName);
    
    ValueTask ToggleDropDownMenu();

    ValueTask Next();
    ValueTask NextFolder();
    ValueTask NextArchive();
    ValueTask Last();
    ValueTask Prev();
    ValueTask PrevFolder();
    ValueTask PrevArchive();
    ValueTask First();
    ValueTask Next10();
    ValueTask Next100();
    ValueTask Prev10();
    ValueTask Prev100();
    ValueTask StopRepeatedNavigation();
    
    ValueTask Search();
    ValueTask Up();
    ValueTask RotateRight();
    ValueTask RotateLeft();
    ValueTask Down();
    ValueTask ScrollDown();
    ValueTask ScrollUp();
    ValueTask ScrollToTop();
    ValueTask ScrollToBottom();
    ValueTask ZoomIn();
    ValueTask ZoomOut();
    ValueTask ResetZoom();

    ValueTask ToggleScroll();
    ValueTask ChangeCtrlZoom();
    ValueTask ToggleLooping();
    ValueTask ToggleInterface();
    ValueTask ToggleSubdirectories();
    ValueTask ToggleBottomToolbar();
    ValueTask ToggleTaskbarProgress();
    ValueTask ToggleConstrainBackgroundColor();

    ValueTask ToggleGallery();
    ValueTask OpenCloseBottomGallery();
    ValueTask CloseGallery();
    ValueTask GalleryClick();

    ValueTask ShowStartUpMenu();
    ValueTask Close();
    ValueTask Center();
    ValueTask Maximize();
    ValueTask Restore();
    ValueTask NewWindow();
    ValueTask AboutWindow();
    ValueTask ConvertWindow();
    ValueTask KeybindingsWindow();
    ValueTask EffectsWindow();
    ValueTask ImageInfoWindow();
    ValueTask ResizeWindow();
    ValueTask BatchResizeWindow();
    ValueTask SettingsWindow();

    ValueTask Stretch();
    ValueTask AutoFitWindow();
    ValueTask NormalWindow();
    ValueTask ToggleFullscreen();
    ValueTask Fullscreen();
    ValueTask SetTopMost();

    ValueTask OpenLastFile();
    ValueTask OpenPreviousFileHistoryEntry();
    ValueTask OpenNextFileHistoryEntry();
    ValueTask Print();
    ValueTask Open();
    ValueTask OpenWith();
    ValueTask OpenInExplorer();
    ValueTask Save();
    ValueTask SaveAs();
    ValueTask DeleteFile();
    ValueTask DeleteFilePermanently();
    ValueTask Rename();
    ValueTask ShowFileProperties();

    ValueTask CopyFile();
    ValueTask CopyFilePath();
    ValueTask CopyImage();
    ValueTask CopyBase64();
    ValueTask DuplicateFile();
    ValueTask CutFile();
    ValueTask Paste();

    ValueTask ChangeBackground();
    ValueTask SideBySide();
    ValueTask Reload();
    ValueTask ResizeImage();
    ValueTask Crop();
    ValueTask Flip();
    ValueTask OptimizeImage();
    ValueTask Slideshow();
    ValueTask ColorPicker();

    ValueTask SortFilesByName();
    ValueTask SortFilesByCreationTime();
    ValueTask SortFilesByLastAccessTime();
    ValueTask SortFilesByLastWriteTime();
    ValueTask SortFilesBySize();
    ValueTask SortFilesByExtension();
    ValueTask SortFilesRandomly();
    ValueTask SortFilesAscending();
    ValueTask SortFilesDescending();

    ValueTask Set0Star();
    ValueTask Set1Star();
    ValueTask Set2Star();
    ValueTask Set3Star();
    ValueTask Set4Star();
    ValueTask Set5Star();

    ValueTask OpenGoogleMaps();
    ValueTask OpenBingMaps();

    ValueTask SetAsWallpaper();
    ValueTask SetAsWallpaperTiled();
    ValueTask SetAsWallpaperCentered();
    ValueTask SetAsWallpaperStretched();
    ValueTask SetAsWallpaperFitted();
    ValueTask SetAsWallpaperFilled();
    ValueTask SetAsLockscreenCentered();
    ValueTask SetAsLockScreen();

    ValueTask NewTab();
    ValueTask CloseTab();

    ValueTask ResetSettings();
    ValueTask Restart();
    ValueTask ShowSettingsFile();
    ValueTask ShowKeybindingsFile();
    ValueTask ShowRecentHistoryFile();
    ValueTask ToggleOpeningInSameWindow();
    ValueTask ToggleFileHistory();
}
