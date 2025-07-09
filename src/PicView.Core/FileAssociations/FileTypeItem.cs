using R3;

namespace PicView.Core.FileAssociations;

public class FileTypeItem : IDisposable
{
    public string Description { get; }
    public string[] Extensions { get; }
    
    public string Extension => string.Join(", ", Extensions);

    public BindableReactiveProperty<bool?> IsSelected { get; } = new();

    public BindableReactiveProperty<bool> IsVisible { get; } = new(true);

    public FileTypeItem(string description, string[] extensions, bool? isSelected = true)
    {
        Description = description;
        Extensions = extensions;
        IsSelected.Value = isSelected;
    }

    public void Dispose()
    {
        Disposable.Dispose(IsSelected, IsVisible);
    }
}