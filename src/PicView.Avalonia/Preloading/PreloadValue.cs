using PicView.Avalonia.ImageHandling;

namespace PicView.Avalonia.Preloading;

/// <summary>
/// Represents a preloaded image value.
/// </summary>
public class PreLoadValue
{
    private TaskCompletionSource<bool>? _loadingCompletionSource;
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreLoadValue"/> class.
    /// </summary>
    /// <param name="imageModel">The image model.</param>
    /// <param name="isLoading">Indicates whether the image is loading.</param>
    public PreLoadValue(ImageModel imageModel, bool isLoading = false)
    {
        ImageModel = imageModel;
        if (isLoading)
        {
            _loadingCompletionSource = new TaskCompletionSource<bool>();
        }
        IsLoading = isLoading;
    }

    /// <summary>
    /// Gets or sets the image model.
    /// </summary>
    public ImageModel ImageModel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image is loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            var wasLoading = _isLoading;
            _isLoading = value;
            
            // Signal completion when loading changes from true to false
            if (wasLoading && !value && _loadingCompletionSource != null)
            {
                _loadingCompletionSource.TrySetResult(true);
                _loadingCompletionSource = null;
            }
            else if (value && !wasLoading)
            {
                // If we're starting to load, create a new completion source
                _loadingCompletionSource = new TaskCompletionSource<bool>();
            }
        }
    }

    /// <summary>
    /// Gets a task that completes when loading is finished.
    /// </summary>
    /// <returns>A task that completes when IsLoading becomes false.</returns>
    public Task WaitForLoadingCompleteAsync()
    {
        return !_isLoading ? Task.CompletedTask : _loadingCompletionSource?.Task ?? Task.CompletedTask;
    }
}