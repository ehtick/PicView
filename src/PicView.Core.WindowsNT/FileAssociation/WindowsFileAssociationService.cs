using PicView.Core.FileAssociations;

namespace PicView.Core.WindowsNT.FileAssociation;

/// <summary>
/// Windows-specific implementation of IFileAssociationService
/// </summary>
public class WindowsFileAssociationService : IFileAssociationService
{
    public async Task<bool> RegisterFileAssociation(string extension, string description) =>
        await Task.Run(() => WindowsFileAssociation.RegisterFileAssociation(extension, description));
    
    public async Task<bool> UnregisterFileAssociation(string extension) =>
        await Task.Run(() => WindowsFileAssociation.UnregisterFileAssociation(extension));
    
    public async Task<bool> IsFileAssociated(string extension) =>
        await Task.Run(() => WindowsFileAssociation.IsFileAssociated(extension));
}