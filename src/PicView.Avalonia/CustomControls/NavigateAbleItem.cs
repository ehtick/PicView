using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace PicView.Avalonia.CustomControls;

[PseudoClasses(PseudoCurrentItem, PseudoSelectedItem)]
public class NavigateAbleItem : ContentControl
{
    private const string PseudoCurrentItem = ":currentItem";
    private const string PseudoSelectedItem = ":selectedItem";
    
    protected override Type StyleKeyOverride => typeof(NavigateAbleItem);

    public void SetCurrent(bool isCurrent)
    {
        PseudoClasses.Set(PseudoCurrentItem, isCurrent);
    }
    
    public void SetSelected(bool isSelected)
    {
        PseudoClasses.Set(PseudoSelectedItem, isSelected);
    }
}