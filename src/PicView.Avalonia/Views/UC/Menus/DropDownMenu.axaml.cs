using Avalonia.Interactivity;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ObservableCollections;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class DropDownMenu : AnimatedMenu
{
    private IDisposable? _menuVisibilitySubscription;

    public DropDownMenu()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SlideShow2Sec.Text = $"2 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow5Sec.Text = $"5 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow10Sec.Text = $"10 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow20Sec.Text = $"20 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow30Sec.Text = $"30 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow60Sec.Text = $"60 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow90Sec.Text = $"90 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow120Sec.Text = $"120 {TranslationManager.Translation.SecAbbreviation}";

        if (DataContext is MainWindowViewModel vm)
        {
            vm.FileHistory.PinnedEntries.CollectionChanged += PinnedEntriesOnCollectionChanged;
            vm.FileHistory.UnpinnedEntries.CollectionChanged += UnPinnedEntriesOnCollectionChanged;   
            
            _menuVisibilitySubscription = vm.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible
                .Subscribe(isVisible =>
                {
                    if (isVisible)
                    {
                        MaxHeight = UIHelper2.GetMainView.Bounds.Height - 1;
                        vm.FileHistory.UpdateHistory();
                    }
                }, static result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(DropDownMenu), nameof(_menuVisibilitySubscription), result.Exception);
                    }
#endif
                });
        }
    }

    private void PinnedEntriesOnCollectionChanged(in NotifyCollectionChangedEventArgs<FileHistoryEntryViewModel> e)
    {
        UpdateCollection(e, PinnedEntriesCollection);
    }
    
    private void UnPinnedEntriesOnCollectionChanged(in NotifyCollectionChangedEventArgs<FileHistoryEntryViewModel> e)
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
        if (DataContext is MainWindowViewModel vm)
        {
            vm.FileHistory.PinnedEntries.CollectionChanged -= PinnedEntriesOnCollectionChanged;
            vm.FileHistory.UnpinnedEntries.CollectionChanged -= UnPinnedEntriesOnCollectionChanged;   
        }
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