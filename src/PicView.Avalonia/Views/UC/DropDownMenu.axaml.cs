using Avalonia.Interactivity;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using ObservableCollections;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Views.UC;

public partial class DropDownMenu : AnimatedMenu
{
    private IDisposable? _menuVisibilitySubscription;

    public DropDownMenu()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        core.FileHistory ??= new FileHistoryViewModel(core);
        DataContext = core;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!Settings.Theme.Dark)
        {
            ApplyLightTheme();
        }
        
        SlideShow2Sec.Text = $"2 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow5Sec.Text = $"5 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow10Sec.Text = $"10 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow20Sec.Text = $"20 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow30Sec.Text = $"30 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow60Sec.Text = $"60 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow90Sec.Text = $"90 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow120Sec.Text = $"120 {TranslationManager.Translation.SecAbbreviation}";

        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        core.FileHistory.PinnedEntries.CollectionChanged += PinnedEntriesOnCollectionChanged;
        core.FileHistory.Entries.CollectionChanged += EntriesOnCollectionChanged;

        _menuVisibilitySubscription = Observable.EveryValueChanged(this, x => x.IsVisible)
            .SubscribeOn(UIHelper.GetFrameProvider).Subscribe(isVisible =>
            {
                if (isVisible)
                {
                    MaxHeight = UIHelper.GetMainView.Bounds.Height - 1;
                    core.FileHistory.UpdateHistory();
                }
                else
                {
                    // Reset it, so that it opens in default state the next time it opens
                    core.MainWindows.ActiveWindow.Value.TopTitlebarViewModel.DropDownMenu.CloseToDefault();
                }
            }, DebugHelper.LogError(nameof(DropDownMenu), nameof(_menuVisibilitySubscription)));
    }

    private void ApplyLightTheme()
    {
        var white = Brushes.White;
        MainBorder.Background = white;

        // Gallery
        MainGalleryButton.Classes.Remove("altHover");
        MainGalleryButton.Classes.Add("hover");

        GalleryExpandButton.Background = white;
        GalleryExpandButton.Classes.Remove("noBorderHover");
        GalleryExpandButton.Classes.Add("hover");
        
        GalleryContractButton.Background = white;
        GalleryContractButton.Classes.Remove("noBorderHover");
        GalleryContractButton.Classes.Add("hover");
        
        BottomDockButton.Classes.Remove("altHover");
        BottomDockButton.Classes.Add("hover");
        
        TopDockButton.Classes.Remove("altHover");
        TopDockButton.Classes.Add("hover");
        
        RightDockButton.Classes.Remove("altHover");
        RightDockButton.Classes.Add("hover");
        
        LeftDockButton.Classes.Remove("altHover");
        LeftDockButton.Classes.Add("hover");
        
        HideDockedButton.Classes.Remove("altHover");
        HideDockedButton.Classes.Add("hover");
        
        // Slideshow
        MainSlideshowButton.Classes.Remove("altHover");
        MainSlideshowButton.Classes.Add("hover");
        
        SlideshowExpandButton.Background = white;
        SlideshowExpandButton.Classes.Remove("noBorderHover");
        SlideshowExpandButton.Classes.Add("hover");
        
        SlideshowContractButton.Background = white;
        SlideshowContractButton.Classes.Remove("noBorderHover");
        SlideshowContractButton.Classes.Add("hover");
        
        SlideShow2Sec.Classes.Remove("altHover");
        SlideShow2Sec.Classes.Add("hover");
        
        SlideShow5Sec.Classes.Remove("altHover");
        SlideShow5Sec.Classes.Add("hover");
        
        SlideShow10Sec.Classes.Remove("altHover");
        SlideShow10Sec.Classes.Add("hover");
        
        SlideShow20Sec.Classes.Remove("altHover");
        SlideShow20Sec.Classes.Add("hover");
        
        SlideShow30Sec.Classes.Remove("altHover");
        SlideShow30Sec.Classes.Add("hover");
        
        SlideShow60Sec.Classes.Remove("altHover");
        SlideShow60Sec.Classes.Add("hover");
        
        SlideShow90Sec.Classes.Remove("altHover");
        SlideShow90Sec.Classes.Add("hover");
        
        SlideShow120Sec.Classes.Remove("altHover");
        SlideShow120Sec.Classes.Add("hover");
        
        // Tool windows
        ToolWindowsExpandButton.Background = white;
        ToolWindowsExpandButton.Classes.Remove("noBorderHover");
        ToolWindowsExpandButton.Classes.Add("hover");
        
        ToolWindowButton.Classes.Remove("altHover");
        ToolWindowButton.Classes.Add("hover");
        
        ToolWindowContractButton.Classes.Remove("noBorderHover");
        ToolWindowContractButton.Classes.Add("hover");
        
        EffectsButton.Classes.Remove("altHover");
        EffectsButton.Classes.Add("hover");
        
        SingleImageResizeButton.Classes.Remove("altHover");
        SingleImageResizeButton.Classes.Add("hover");
        
        BatchResizeButton.Classes.Remove("altHover");
        BatchResizeButton.Classes.Add("hover");
        
        ImageInfoButton.Classes.Remove("altHover");
        ImageInfoButton.Classes.Add("hover");     
        
        CropButton.Classes.Remove("altHover");
        CropButton.Classes.Add("hover");
        
        // Settings 
        SettingsButton.Classes.Remove("altHover");
        SettingsButton.Classes.Add("hover");
        
        SettingsExpandButton.Background = white;
        SettingsExpandButton.Classes.Remove("noBorderHover");
        SettingsExpandButton.Classes.Add("hover");
        
        SettingsContractButton.Background = white;
        SettingsContractButton.Classes.Remove("noBorderHover");
        SettingsContractButton.Classes.Add("hover");
        
        AutoFitButton.Classes.Remove("altHover");
        AutoFitButton.Classes.Add("hover");
                
        TopMostButton.Classes.Remove("altHover");
        TopMostButton.Classes.Add("hover");
                        
        LoopingButton.Classes.Remove("altHover");
        LoopingButton.Classes.Add("hover");
        
        ScrollButton.Classes.Remove("altHover");
        ScrollButton.Classes.Add("hover");
        
        StretchButton.Classes.Remove("altHover");
        StretchButton.Classes.Add("hover");
        
        SideBySideButton.Classes.Remove("altHover");
        SideBySideButton.Classes.Add("hover");
        
        AboutButton.Classes.Remove("altHover");
        AboutButton.Classes.Add("hover");
                
        KeybindingsButton.Classes.Remove("altHover");
        KeybindingsButton.Classes.Add("hover");
        
        AllSettingsButton.Classes.Remove("altHover");
        AllSettingsButton.Classes.Add("hover");

    }

    private void PinnedEntriesOnCollectionChanged(in NotifyCollectionChangedEventArgs<FileHistoryEntryViewModel> e)
    {
        UpdateCollection(e, PinnedEntriesCollection);
    }
    
    private void EntriesOnCollectionChanged(in NotifyCollectionChangedEventArgs<FileHistoryEntryViewModel> e)
    {
        UpdateCollection(e, UnPinnedEntriesCollection);
    }

    private void UpdateCollection(in NotifyCollectionChangedEventArgs<FileHistoryEntryViewModel> e, ItemsControl collection)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.IsSingleItem)
                {
                    var newItem = e.NewItem;
                    Dispatcher.CurrentDispatcher.Post(() => { collection.Items.Add(newItem); },
                        DispatcherPriority.Background);
                }
                else
                {
                    foreach (var item in e.NewItems)
                    {
                        Dispatcher.CurrentDispatcher.Post(() => { collection.Items.Add(item); },
                            DispatcherPriority.Background);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.IsSingleItem)
                {
                    var removedItem = e.OldItem;
                    Dispatcher.CurrentDispatcher.Post(() => { collection.Items.Remove(removedItem); },
                        DispatcherPriority.Background);
                }
                else
                {
                    foreach (var item in e.OldItems)
                    {
                        Dispatcher.CurrentDispatcher.Post(() => { collection.Items.Remove(item); },
                            DispatcherPriority.Background);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                collection.Items.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        _menuVisibilitySubscription?.Dispose();
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        core.FileHistory.PinnedEntries.CollectionChanged -= PinnedEntriesOnCollectionChanged;
        core.FileHistory.Entries.CollectionChanged -= EntriesOnCollectionChanged;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        // Trigger closing animation
        IsOpen = false;
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        // Let view model know it is closed
        vm.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
    }
}