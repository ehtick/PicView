using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryItemSizeSlider : UserControl
{
    public GalleryItemSizeSlider()
    {
        InitializeComponent();
    }
    
    public void SetMaxAndMin()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            CustomSlider.Maximum = GalleryDefaults.MaxFullGalleryItemHeight;
            CustomSlider.Minimum = GalleryDefaults.MinBottomGalleryItemHeight;
        }
        else
        {
            CustomSlider.Maximum = GalleryDefaults.MaxBottomGalleryItemHeight;
            CustomSlider.Minimum = GalleryDefaults.MinBottomGalleryItemHeight;
        }
    }

    private void RangeBase_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vm.Gallery.GalleryItem.ItemHeight.CurrentValue == e.NewValue)
            {
                return;
            }
            vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value = e.NewValue;
            
            WindowResizing.SetSize(vm);
            // TODO: Binding to height depends on timing of the update. Maybe find a cleaner mvvm solution one day
        
            // Maybe save this on close or some other way
            Settings.Gallery.ExpandedGalleryItemSize = e.NewValue;
            
        }
        else if (Settings.Gallery.IsBottomGalleryShown)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue == e.NewValue)
            {
                return;
            }
            vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = e.NewValue;
            
            UIHelper.GetGalleryView.Height = GalleryFunctions.GetGalleryHeight(vm);
            WindowResizing.SetSize(vm);
        
            // Binding to height depends on timing of the update. Maybe find a cleaner mvvm solution one day
            // Maybe save this on close or some other way
            Settings.Gallery.BottomGalleryItemSize = e.NewValue;
        }
        
        vm.Gallery.GalleryItem.ItemHeight.Value = e.NewValue;
       
        _ = SaveSettingsAsync();
    }
}
