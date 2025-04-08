using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Core.Localization;
using Path = Avalonia.Controls.Shapes.Path;

namespace PicView.Avalonia.CustomControls;

public class FuncTextBox : TextBox
{
    private bool _contextMenuLoaded;
        
    public FuncTextBox()
    {
        ContextMenu = new ContextMenu();

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                ContextMenu.Open(this);
            }
        };

        ContextMenu.Opening += (_, _) =>
        {
            if (!_contextMenuLoaded)
            {
                LoadContextMenu();
            }
        };

        ContextMenu.Opened += (_, _) =>
        {
            Classes.Add("active");
        };

        ContextMenu.Closed += (_, _) =>
        {
            Classes.Remove("active");
        };
    }

    protected override Type StyleKeyOverride => typeof(TextBox);

    private void LoadContextMenu()
    {
        if (!Application.Current.TryGetResource("MainTextColor", Application.Current.RequestedThemeVariant, out var mainTextColor))
        {
            return;
        }

        var iconBrush = new SolidColorBrush((Color)(mainTextColor ?? Brushes.White));
        if (!Application.Current.TryGetResource("CopyGeometry", Application.Current.RequestedThemeVariant, out var copyGeometry))
        {
            return;
        }

        if (!Application.Current.TryGetResource("CutGeometry", Application.Current.RequestedThemeVariant, out var cutGeometry))
        {
            return;
        }

        if (!Application.Current.TryGetResource("RecycleGeometry", Application.Current.RequestedThemeVariant, out var recycleGeometry))
        {
            return;
        }

        if (!Application.Current.TryGetResource("PasteGeometry", Application.Current.RequestedThemeVariant, out var pasteGeometry))
        {
            return;
        }

        if (!Application.Current.TryGetResource("CheckboxOutlineImage", Application.Current.RequestedThemeVariant,
                out var checkboxOutlineImage))
        {
            return;
        }
            
        var selectAllMenuItem = new MenuItem
        {
            Header = TranslationManager.Translation.SelectAll,
            Icon = new Image
            {
                Width = 12,
                Height = 12,
                Source = checkboxOutlineImage as DrawingImage ?? null
            }
        };
        selectAllMenuItem.Click += (_, _) => SelectAll();
        ContextMenu.Items.Add(selectAllMenuItem);

        var cutMenuItem = new MenuItem
        {
            Header = TranslationManager.Translation.Cut,
            Icon = new Path
            {
                Width = 12,
                Height = 12,
                Fill = iconBrush,
                Stretch = Stretch.Fill,
                Data = cutGeometry as Geometry ?? null,
            }
        };
        cutMenuItem.Click += (_, _) => Cut();
        ContextMenu.Items.Add(cutMenuItem);

        var copyMenuItem = new MenuItem
        {
            Header = TranslationManager.Translation.Copy,
            Icon = new Path
            {
                Width = 12,
                Height = 12,
                Fill = iconBrush,
                Stretch = Stretch.Fill,
                Data = copyGeometry as Geometry ?? null
            },
        };
        copyMenuItem.Click += (_, _) => Copy();
        ContextMenu.Items.Add(copyMenuItem);

        var pasteMenuItem = new MenuItem
        {
            Header = TranslationManager.Translation.FilePaste,
            Icon = new Path
            {
                Width = 12,
                Height = 12,
                Fill = iconBrush,
                Stretch = Stretch.Fill,
                Data = pasteGeometry as Geometry ?? null
            }
        };
        pasteMenuItem.Click += (_, _) => Paste();
        ContextMenu.Items.Add(pasteMenuItem);

        var deleteMenuItem = new MenuItem
        {
            Header = TranslationManager.Translation.DeleteFile,
            Icon = new Path
            {
                Width = 12,
                Height = 12,
                Fill = iconBrush,
                Stretch = Stretch.Fill,
                Data = recycleGeometry as Geometry ?? null
            }
        };
        deleteMenuItem.Click += (_, _) => Clear();
        ContextMenu.Items.Add(deleteMenuItem);

        ContextMenu.Opened += delegate
        {
            if (IsReadOnly)
            {
                deleteMenuItem.IsEnabled = false;
                cutMenuItem.IsEnabled = false;
                pasteMenuItem.IsEnabled = false;
            }
            else
            {
                deleteMenuItem.IsEnabled = true;
                cutMenuItem.IsEnabled = true;
                pasteMenuItem.IsEnabled = true;
            }
        };
            
        _contextMenuLoaded = true;
    }
}