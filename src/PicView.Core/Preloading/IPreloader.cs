using PicView.Core.Models;

namespace PicView.Core.Preloading;

/// <summary>
/// Defines the contract for the background worker.
/// <para>
/// Implementations of this interface are responsible for calculating predictive indices 
/// (look-ahead) and executing the physical loading of images into the cache.
/// </para>
/// </summary>
public interface IPreloader
{
    /// <summary>
    /// Adds a fully loaded item directly to the cache.
    /// </summary>
    void Add(string ownerId, int index, ImageModel model, IReadOnlyList<FileInfo> list);

    /// <summary>
    /// Asynchronously loads an item and adds it to the cache.
    /// </summary>
    ValueTask<ImageModel?> AddAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, bool isReverse = false,
        CancellationToken ct = default);
    
    void Resynchronize(IReadOnlyList<FileInfo> files);
    
    /// <summary>
    /// Starts the predictive loop to load images ahead of the current view.
    /// </summary>
    void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files);

    /// <summary>
    /// Registers a new owner to track cancellation tokens for their specific load tasks.
    /// </summary>
    void RegisterOwner(string ownerId);

    /// <summary>
    /// Cancels all running preload tasks for a specific owner.
    /// </summary>
    ValueTask CancelOwnerInstanceAsync(string ownerId);
}