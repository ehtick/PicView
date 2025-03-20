using PicView.Core.FileHandling;

namespace PicView.Core.WindowsNT.FileAssociation;

/// <summary>
/// Windows-specific implementation of IFileAssociationService
/// </summary>
public class WindowsFileAssociationService : IFileAssociationService
{
    public async Task<bool> RegisterFileAssociation(string extension, string description) =>
        await Task.Run(() => FileAssociationHelper.RegisterFileAssociation(extension, description));
    
    public async Task<bool> UnregisterFileAssociation(string extension) =>
        await Task.Run(() => FileAssociationHelper.UnregisterFileAssociation(extension));
    
    public async Task<bool> IsFileAssociated(string extension) =>
        await Task.Run(() => FileAssociationHelper.IsFileAssociated(extension));
}