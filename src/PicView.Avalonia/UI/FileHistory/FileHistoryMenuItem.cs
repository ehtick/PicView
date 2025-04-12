using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC.Buttons;
using PicView.Core.Extensions;
using PicView.Core.FileHistory;

namespace PicView.Avalonia.UI.FileHistory
{
    /// <summary>
    ///     Represents a single file history menu item with pin/unpin functionality
    /// </summary>
    public class FileHistoryMenuItem : Panel
    {
        private const int MaxFilenameLength = 42;

        public FileHistoryMenuItem(Entry entry, string? currentFilePath, MainViewModel viewModel, int index)
        {
            var fileLocation = entry.Path;
            if (string.IsNullOrEmpty(fileLocation))
            {
                return;
            }

            var isSelected = fileLocation == currentFilePath;
            var filename = Path.GetFileName(fileLocation);
            var header = filename.Length > MaxFilenameLength ? filename.Shorten(MaxFilenameLength) : filename;

            // Create the pin button with appropriate visibility
            var pinButton = CreatePinButton(entry, fileLocation);

            // Create the menu item button with file info
            var menuItemButton = CreateMenuItemButton(header, fileLocation, isSelected, index, viewModel);

            // Add components to the panel
            Children.Add(menuItemButton);
            Children.Add(pinButton);

            // Add hover behavior
            ConfigureHoverBehavior(pinButton);

            // Set tooltip
            ToolTip.SetTip(menuItemButton, fileLocation);
        }

        private static PinButton CreatePinButton(Entry entry, string fileLocation)
        {
            var pinBtn = new PinButton
            {
                Opacity = 0,
                Width = 25,
                HorizontalAlignment = HorizontalAlignment.Right,
                ZIndex = 1
            };

            // Toggle just this button's visibility instead of rebuilding the whole menu
            pinBtn.PinBtn.Click += (_, _) => 
            {
                FileHistoryManager.Pin(fileLocation);
                // Just update this button's visibility
                pinBtn.PinBtn.IsVisible = false;
                pinBtn.UnPinBtn.IsVisible = true;
        
                // Schedule a deferred menu refresh to happen after this event has completed
                Dispatcher.UIThread.Post(() =>
                {
                    UIHelper.GetMainView.FileHistoryMenuController.UpdateFileHistoryMenu();
                });
            };
    
            pinBtn.UnPinBtn.Click += (_, _) => 
            {
                FileHistoryManager.UnPin(fileLocation);
                // Just update this button's visibility
                pinBtn.PinBtn.IsVisible = true;
                pinBtn.UnPinBtn.IsVisible = false;
        
                // Schedule a deferred menu refresh to happen after this event has completed
                Dispatcher.UIThread.Post(() =>
                {
                    UIHelper.GetMainView.FileHistoryMenuController.UpdateFileHistoryMenu();
                });
            };

            if (entry.IsPinned)
            {
                pinBtn.PinBtn.IsVisible = false;
                pinBtn.UnPinBtn.IsVisible = true;
            }
            else
            {
                pinBtn.PinBtn.IsVisible = true;
                pinBtn.UnPinBtn.IsVisible = false;
            }

            return pinBtn;
        }

        private static Button CreateMenuItemButton(string header, string fileLocation, bool isSelected, int index,
            MainViewModel viewModel)
        {
            var item = new Button
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(5, 6),
                Width = 355
            };

            if (index < 0)
            {
                // Pinned item without index number
                item.Padding = new Thickness(15, 0, 0, 0);
                item.Content = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 5, 0, 5)
                };
            }
            else
            {
                // Regular item with index number
                var indexText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = (index + 1).ToString(),
                    Padding = new Thickness(5, 0, 2, 0)
                };

                var headerText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 0, 0, 0)
                };

                item.Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { indexText, headerText }
                };
            }

            if (isSelected)
            {
                item.Classes.Add("active");
            }

            item.Click += async delegate
            {
                UIHelper.GetMainView.MainContextMenu.Close();
                await NavigationManager.LoadPicFromStringAsync(fileLocation, viewModel).ConfigureAwait(false);
            };

            return item;
        }

        private void ConfigureHoverBehavior(PinButton pinBtn)
        {
            PointerEntered += (_, _) =>
            {
                pinBtn.Opacity = 1;
                if (Application.Current.TryGetResource("AccentColor", Application.Current.RequestedThemeVariant,
                        out var accentColor))
                {
                    Background = accentColor as SolidColorBrush;
                }
            };

            PointerExited += (_, _) =>
            {
                pinBtn.Opacity = 0;
                Background = Brushes.Transparent;
            };
        }
    }
}