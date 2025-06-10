using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;

namespace PicView.Core.Navigation;

/// <summary>
/// Provides methods for determining the type of file or resource from a given input string
/// and encapsulates logic to classify the resource as a loadable entity.
/// </summary>
public static class FileTypeResolver
{
    /// <summary>
    /// Represents a structure containing a loadable file type and its associated data.
    /// </summary>
    /// <param name="type">The type of the loadable file.</param>
    /// <param name="data">The data associated with the loadable file.</param>
    public readonly struct FileTypeStruct(LoadAbleFileType type, string data)
    {
        /// <summary>
        /// Gets the type of the loadable file.
        /// </summary>
        public LoadAbleFileType Type => type;
        
        /// <summary>
        /// Gets the data associated with the loadable file.
        /// </summary>
        public string Data => data;
    }

    /// <summary>
    /// Specifies the different types of loadable files.
    /// </summary>
    public enum LoadAbleFileType
    {
        /// <summary>
        /// Represents a regular file.
        /// </summary>
        File,
        
        /// <summary>
        /// Represents a directory.
        /// </summary>
        Directory,
        
        /// <summary>
        /// Represents a web URL.
        /// </summary>
        Web,
        
        /// <summary>
        /// Represents a Base64 encoded string.
        /// </summary>
        Base64,
        
        /// <summary>
        /// Represents a zip archive.
        /// </summary>
        Zip
    }
    
    /// <summary>
    /// Checks if the provided string is a loadable file type and returns its type and associated data.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>
    /// A <see cref="FileTypeStruct"/> containing the type and data of the loadable file if the string is loadable, otherwise null.
    ///fd </returns>
    public static FileTypeStruct? CheckIfLoadableString(string s)
    {
        if (s.StartsWith('"') && s.EndsWith('"'))
        {
            s = s[1..^1];
        }
        
        if (s.StartsWith("file:///"))
        {
            s = s.Replace("file:///", "");
            s = s.Replace("%20", " ");
        }

        if (File.Exists(s))
        {
            var type = s.IsArchive() ? LoadAbleFileType.Zip : LoadAbleFileType.File;
            return new FileTypeStruct(type, s);
        }

        if (Directory.Exists(s))
        {
            return new FileTypeStruct(LoadAbleFileType.Directory, s);
        }

        if (!string.IsNullOrWhiteSpace(s.GetURL()))
        {
            return new FileTypeStruct(LoadAbleFileType.Web, s);
        }

        var base64String = Base64Decoder.IsBase64String(s);

        if (!string.IsNullOrEmpty(base64String))
        {
            return new FileTypeStruct(LoadAbleFileType.Base64, base64String);
        }

        return null;
    }
}