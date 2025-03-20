namespace PicView.Core.FileHandling;

/// <summary>
/// Platform-agnostic manager for file associations that delegates to platform-specific implementations
/// </summary>
public static class FileAssociationManager
{
    private static IFileAssociationService? _service;
    
    /// <summary>
    /// Initializes the FileAssociationManager with the appropriate platform-specific implementation
    /// </summary>
    /// <param name="service">Platform-specific implementation of IFileAssociationService</param>
    public static void Initialize(IFileAssociationService? service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
    
    /// <summary>
    /// Associates all supported file extensions with the application
    /// </summary>
    public static async Task AssociateAllSupportedFiles()
    {
        EnsureInitialized();
        
        foreach (var ext in SupportedFiles.FileExtensions)
        {
            await _service.RegisterFileAssociation(ext, $"{ext.TrimStart('.')} Image File");
        }
    }
    
    /// <summary>
    /// Removes all file associations for supported file extensions
    /// </summary>
    public static async Task UnassociateAllSupportedFiles()
    {
        EnsureInitialized();
        
        foreach (var ext in SupportedFiles.FileExtensions)
        {
            await _service.UnregisterFileAssociation(ext);
        }
    }

    /// <summary>
    /// Associates a single file extension with the application
    /// </summary>
    public static async Task<bool> AssociateFile(string fileExtension)
    {
        EnsureInitialized();
        return await _service.RegisterFileAssociation(fileExtension, $"{fileExtension.TrimStart('.')} Image File");
    }
    
    /// <summary>
    /// Unassociates a single file extension
    /// </summary>
    public static async Task<bool> UnassociateFile(string fileExtension)
    {
        EnsureInitialized();
        return await _service.UnregisterFileAssociation(fileExtension);
    }
    
    /// <summary>
    /// Checks if a file extension is associated with the application
    /// </summary>
    public static async Task<bool> IsFileAssociated(string fileExtension)
    {
        EnsureInitialized();
        return await _service.IsFileAssociated(fileExtension);
    }
    
    /// <summary>
    /// Gets the association status of all supported file extensions
    /// </summary>
    public static async Task<Dictionary<string, bool>> GetAllAssociationStatus()
    {
        EnsureInitialized();
        var result = new Dictionary<string, bool>();
        
        foreach (var ext in SupportedFiles.FileExtensions)
        {
            result[ext] = await _service.IsFileAssociated(ext);
        }
        
        return result;
    }
    
    private static void EnsureInitialized()
    {
        if (_service == null)
        {
            throw new InvalidOperationException(
                "FileAssociationManager has not been initialized. Call Initialize() with an appropriate implementation before using this class.");
        }
    }
}