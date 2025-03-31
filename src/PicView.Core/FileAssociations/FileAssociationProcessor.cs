using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using PicView.Core.FileHandling;
using PicView.Core.ProcessHandling;

namespace PicView.Core.FileAssociations;

/// <summary>
/// Handles the processing of file association operations, including elevation for Windows and handling command-line arguments.
/// </summary>
public static class FileAssociationProcessor
{
    /// <summary>
    /// Sets file associations for a collection of file type groups.
    /// On Windows, elevates permissions if necessary.
    /// </summary>
    /// <param name="groups">Collection of file type groups to process</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> SetFileAssociations(ReadOnlyObservableCollection<FileTypeGroup> groups)
    {
        try
        {
            // If we're on Windows, check for admin permissions
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsAdministrator())
            {
                return await HandleNonAdminWindowsAssociations(groups);
            }
            
            // Standard processing path (non-Windows or already has admin rights)
            return await HandleDirectAssociations(groups);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Debug.WriteLine($"Error in SetFileAssociations: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Processes command-line arguments for file associations.
    /// </summary>
    /// <param name="arg">The command-line argument to process</param>
    public static async Task ProcessFileAssociationArguments(string arg)
    {
        try
        {
            if (arg.StartsWith("associate:", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessAssociationArgument(arg);
            }
            else if (arg.StartsWith("unassociate:", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessUnassociationArgument(arg);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing file association arguments: {ex.Message}");
            Debug.WriteLine($"Argument was: {arg}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    #region Private Helper Methods

    private static async Task<bool> HandleNonAdminWindowsAssociations(ReadOnlyObservableCollection<FileTypeGroup> groups)
    {
        // Build list of extensions to associate with descriptions
        var extensionsToAssociate = new List<string>();
        var extensionsToUnassociate = new List<string>();

        foreach (var group in groups)
        {
            foreach (var fileType in group.FileTypes)
            {
                if (!fileType.IsSelected.HasValue)
                {
                    continue; // Skip null selections
                }

                foreach (var extension in fileType.Extensions)
                {
                    // Make sure to properly handle extensions that contain commas
                    var individualExtensions =
                        extension.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);

                    foreach (var ext in individualExtensions)
                    {
                        var cleanExt = ext.Trim();
                        if (!cleanExt.StartsWith('.'))
                        {
                            cleanExt = "." + cleanExt;
                        }

                        if (fileType.IsSelected.Value)
                        {
                            // Add to association list
                            extensionsToAssociate.Add($"{cleanExt}|{fileType.Description}");
                        }
                        else
                        {
                            // Add to unassociation list
                            extensionsToUnassociate.Add($"{cleanExt}");
                        }
                    }
                }
            }
        }

        // Build arguments for the elevated process
        var args = new List<string>();

        if (extensionsToAssociate.Count > 0)
        {
            // Create command arguments for associations
            args.Add("associate:" + string.Join(";", extensionsToAssociate));
        }

        if (extensionsToUnassociate.Count > 0)
        {
            // Create command arguments for unassociations
            args.Add("unassociate:" + string.Join(";", extensionsToUnassociate));
        }

        if (args.Count == 0)
        {
            return true; // Nothing to do
        }

        // Start new process with elevated permissions
        return await ProcessHelper.StartProcessWithElevatedPermissionAsync(string.Join(" ", args));
    }

    private static async Task<bool> HandleDirectAssociations(ReadOnlyObservableCollection<FileTypeGroup> groups)
    {
        foreach (var group in groups)
        {
            foreach (var fileType in group.FileTypes)
            {
                if (!fileType.IsSelected.HasValue)
                {
                    continue;
                }

                foreach (var extension in fileType.Extensions)
                {
                    var individualExtensions = extension.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);

                    foreach (var ext in individualExtensions)
                    {
                        var cleanExt = ext.Trim();
                        if (!cleanExt.StartsWith('.'))
                        {
                            cleanExt = "." + cleanExt;
                        }

                        if (fileType.IsSelected.Value)
                        {
                            await FileAssociationManager.AssociateFile(cleanExt, fileType.Description);
                        }
                        else
                        {
                            await FileAssociationManager.UnassociateFile(cleanExt);
                        }
                    }
                }
            }
        }

        return true;
    }

    private static async Task ProcessAssociationArgument(string arg)
    {
        var extensionsString = arg["associate:".Length..];
        if (string.IsNullOrWhiteSpace(extensionsString))
        {
            Debug.WriteLine("No extensions to associate found in arguments.");
            return;
        }

        // Split by semicolons for different extensions
        var extensions = extensionsString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim())
            .ToArray();

        Debug.WriteLine($"Found {extensions.Length} extensions to associate");

        foreach (var extension in extensions)
        {
            try
            {
                // Each extension may have a description after a pipe |
                var parts = extension.Split('|', 2);
                var ext = parts[0].Trim();

                // Get description if available
                string? description = null;
                if (parts.Length > 1)
                {
                    description = parts[1].Trim();
                }

                Debug.WriteLine($"Associating {ext} with description '{description}'");
                await FileAssociationManager.AssociateFile(ext, description);
            }
            catch (Exception extEx)
            {
                Debug.WriteLine($"Error processing extension '{extension}': {extEx.Message}");
            }
        }
    }

    private static async Task ProcessUnassociationArgument(string arg)
    {
        var extensionsString = arg["unassociate:".Length..];
        if (string.IsNullOrWhiteSpace(extensionsString))
        {
            Debug.WriteLine("No extensions to unassociate found in arguments.");
            return;
        }

        // Split by semicolons for different extensions
        var extensions = extensionsString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim())
            .ToArray();

        Debug.WriteLine($"Found {extensions.Length} extensions to unassociate");

        foreach (var extension in extensions)
        {
            try
            {
                // For unassociate, we just need the extension (ignore any description)
                var ext = extension.Split('|')[0].Trim();

                Debug.WriteLine($"Unassociating {ext}");
                await FileAssociationManager.UnassociateFile(ext);
            }
            catch (Exception extEx)
            {
                Debug.WriteLine($"Error unassociating extension '{extension}': {extEx.Message}");
            }
        }
    }

    private static bool IsAdministrator()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        // Check if running as administrator
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    
    #endregion
}