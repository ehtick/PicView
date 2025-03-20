namespace PicView.Core.FileHandling;

/// <summary>
/// Interface for handling platform-specific file associations
/// </summary>
public interface IFileAssociationService
{
    /// <summary>
    /// Registers a file extension to be opened with the application
    /// </summary>
    /// <param name="extension">File extension (with or without leading period)</param>
    /// <param name="description">Description of the file type</param>
    /// <returns>True if registration was successful</returns>
    Task<bool> RegisterFileAssociation(string extension, string description);
    
    /// <summary>
    /// Unregisters a file extension association
    /// </summary>
    /// <param name="extension">File extension (with or without leading period)</param>
    /// <returns>True if unregistration was successful</returns>
    Task<bool> UnregisterFileAssociation(string extension);
    
    /// <summary>
    /// Checks if a file extension is associated with the application
    /// </summary>
    /// <param name="extension">File extension (with or without leading period)</param>
    /// <returns>True if associated, false otherwise</returns>
    Task<bool> IsFileAssociated(string extension);
}