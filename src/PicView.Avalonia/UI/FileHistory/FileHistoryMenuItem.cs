using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC.Buttons;
using PicView.Core.ArchiveHandling;
using PicView.Core.Extensions;
using PicView.Core.FileHistory;

namespace PicView.Avalonia.UI.FileHistory
{
// TODO deprecated, delete
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

            bool isSelected;
            if (ArchiveExtraction.IsArchived)
            {
                isSelected = fileLocation == ArchiveExtraction.LastOpenedArchive;
            }
            else
            {
                isSelected = fileLocation == currentFilePath;
            }
            
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
            
            TextBlock? indexText = null;
            TextBlock? headerText = null;

            if (index < 0)
            {
                // Pinned item without index number
                item.Padding = new Thickness(15, 0, 0, 0);
                headerText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 5, 0, 5)
                };
            }
            else
            {
                // Regular item with index number
                indexText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = (index + 1).ToString(),
                    Padding = new Thickness(5, 0, 2, 0)
                };

                headerText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 0, 0, 0)
                };
            }
            
            if (!Settings.Theme.Dark)
            {
                if (!Application.Current.TryGetResource("MainTextColor",
                        Application.Current.RequestedThemeVariant, out var mainTextColor) ||
                    !Application.Current.TryGetResource("SecondaryTextColor", Application.Current.RequestedThemeVariant, out var secondaryTextColor))
                {
                    throw new InvalidOperationException();
                }

                if (mainTextColor is not Color color || secondaryTextColor is not Color secondaryColor)
                {
                    throw new InvalidOperationException();
                }

                var brush = new SolidColorBrush(color);
                var secondaryBrush = new SolidColorBrush(secondaryColor);
                if (indexText is not null)
                {
                    indexText.Foreground = brush;
                }
                headerText.Foreground = brush;

                item.PointerEntered += delegate
                {
                    if (indexText is not null)
                    {
                        indexText.Foreground = secondaryBrush;
                    }
                    headerText.Foreground = secondaryBrush;
                };
                item.PointerExited += delegate
                {
                    if (indexText is not null)
                    {
                        indexText.Foreground = brush;
                    }
                    headerText.Foreground = brush;
                };
            }

            if (index < 0)
            {
                item.Content = headerText;
            }
            else
            {
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
                if (UIHelper.GetMainView.Resources.TryGetResource("MainContextMenu", Application.Current.ActualThemeVariant, out var value))
                {
                    if (value is ContextMenu mainContextMenu)
                    {
                        mainContextMenu.Close();
                    }
                }
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