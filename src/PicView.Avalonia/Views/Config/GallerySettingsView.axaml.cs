using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using R3;

namespace PicView.Avalonia.Views.Config;

public partial class GalleryView : UserControl
{
    public GalleryView()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            if (Settings.Gallery.FullGalleryStretchMode.Equals("Square",
                    StringComparison.OrdinalIgnoreCase))
            {
                FullGalleryComboBox.SelectedIndex = 4;
            }
            else if (Settings.Gallery.FullGalleryStretchMode.Equals("FillSquare",
                         StringComparison.OrdinalIgnoreCase))
            {
                FullGalleryComboBox.SelectedIndex = 5;
            }
            else if (Enum.TryParse<Stretch>(Settings.Gallery.FullGalleryStretchMode,
                         out var stretchMode))
            {
                FullGalleryComboBox.SelectedIndex = stretchMode switch
                {
                    Stretch.Uniform => 0,
                    Stretch.UniformToFill => 1,
                    Stretch.Fill => 2,
                    Stretch.None => 3,
                    _ => FullGalleryComboBox.SelectedIndex
                };
            }

            if (Settings.Gallery.BottomGalleryStretchMode.Equals("Square",
                    StringComparison.OrdinalIgnoreCase))
            {
                BottomGalleryComboBox.SelectedIndex = 4;
            }
            else if (Settings.Gallery.BottomGalleryStretchMode.Equals("FillSquare",
                         StringComparison.OrdinalIgnoreCase))
            {
                BottomGalleryComboBox.SelectedIndex = 5;
            }
            else if (Enum.TryParse<Stretch>(Settings.Gallery.BottomGalleryStretchMode,
                         out var stretchMode))
            {
                BottomGalleryComboBox.SelectedIndex = stretchMode switch
                {
                    Stretch.Uniform => 0,
                    Stretch.UniformToFill => 1,
                    Stretch.Fill => 2,
                    Stretch.None => 3,
                    _ => FullGalleryComboBox.SelectedIndex
                };
            }

            FullGalleryComboBox.SelectionChanged += (_, _) => FullGalleryComboBox_SelectionChanged();
            BottomGalleryComboBox.SelectionChanged += (_, _) => BottomGalleryComboBox_SelectionChanged();

            BottomGallerySlider.ValueChanged += (_, e) =>
            {
                Settings.Gallery.BottomGalleryItemSize = e.NewValue;
                if (!Settings.Gallery.IsBottomGalleryShown || GalleryFunctions.IsFullGalleryOpen)
                {
                    return;
                }

                vm.Gallery.GalleryItem.ItemHeight.Value = e.NewValue;
                WindowResizing.SetSize(vm);
            };
            ExpandedGallerySlider.ValueChanged += (_, e) =>
            {
                Settings.Gallery.ExpandedGalleryItemSize = e.NewValue;
                if (!GalleryFunctions.IsFullGalleryOpen)
                {
                    return;
                    
                }
                vm.Gallery.GalleryItem.ItemHeight.Value = e.NewValue;
                WindowResizing.SetSize(vm);
            };
        };
    }

    private void FullGalleryComboBox_SelectionChanged()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (FullGalleryUniformItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryItemStretch(vm, Stretch.Uniform);
        }
        else if (FullGalleryUniformToFillItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryItemStretch(vm, Stretch.UniformToFill);
        }
        else if (FullGalleryFillItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryItemStretch(vm, Stretch.Fill);
        }
        else if (FullGalleryNoneItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryItemStretch(vm, Stretch.None);
        }
        else if (FullGallerySquareItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryStretchSquare(vm);
        }
        else if (FullGalleryFillSquareItem.IsSelected)
        {
            GalleryStretchMode.ChangeFullGalleryStretchSquareFill(vm);
        }
    }

    private void BottomGalleryComboBox_SelectionChanged()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (BottomGalleryUniformItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, Stretch.Uniform);
        }
        else if (BottomGalleryUniformToFillItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, Stretch.UniformToFill);
        }
        else if (BottomGalleryFillItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, Stretch.Fill);
        }
        else if (BottomGalleryNoneItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, Stretch.None);
        }
        else if (BottomGallerySquareItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryStretchSquare(vm);
        }
        else if (BottomGalleryFillSquareItem.IsSelected)
        {
            GalleryStretchMode.ChangeBottomGalleryStretchSquareFill(vm);
        }
    }
}