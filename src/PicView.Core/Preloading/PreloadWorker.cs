using System.Threading.Channels;
using PicView.Core.DebugTools;

namespace PicView.Core.Preloading;

/// <summary>
/// A generic worker that handles "Last-Write-Wins" debouncing for navigation events.
/// </summary>
internal class PreloadWorker : IAsyncDisposable
{
    private const int DebounceMs = 80;

    private readonly Channel<PreloadJob> _channel;
    private readonly CancellationTokenSource _lifecycleCts = new();
    private readonly Task _processingTask;
    private readonly Func<PreloadJob, CancellationToken, Task> _workPayload;
    
    // The CTS for the specific batch currently executing
    private CancellationTokenSource? _activeBatchCts;

    public PreloadWorker(Func<PreloadJob, CancellationToken, Task> workPayload)
    {
        _workPayload = workPayload;
        
        // Capacity 1 + DropOldest ensures we only care about the latest navigation target
        _channel = Channel.CreateBounded<PreloadJob>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        _processingTask = Task.Run(ProcessLoopAsync);
    }

    public ChannelWriter<PreloadJob> Writer => _channel.Writer;

    private async Task ProcessLoopAsync()
    {
        var reader = _channel.Reader;

        try
        {
            // Wait for data to be available
            while (await reader.WaitToReadAsync(_lifecycleCts.Token))
            {
                // 1. Drain to get the absolute latest item
                PreloadJob job = default;
                bool hasJob = false;
                while (reader.TryRead(out var item))
                {
                    job = item;
                    hasJob = true;
                }

                if (!hasJob) continue;

                // 2. Debounce: Wait for navigation to settle
                await Task.Delay(DebounceMs, _lifecycleCts.Token);

                // 3. Superseded check: Did new data arrive while we were sleeping?
                if (reader.Count > 0)
                {
                    // Skip execution; next loop iteration will pick up the newer job
                    continue;
                }

                // 4. Execution setup
                CancelActiveBatch();
                _activeBatchCts = CancellationTokenSource.CreateLinkedTokenSource(_lifecycleCts.Token);
                var token = _activeBatchCts.Token;

                // 5. Run the payload (Fire and forget from the loop's perspective, but tracked)
                // We do NOT await this, or we would block the loop from receiving new navigation events.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _workPayload(job, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected on rapid navigation
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogDebug(nameof(PreloadWorker), "BatchExecution", ex);
                    }
                }, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Worker shutting down
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(PreloadWorker), "WorkerLoop", ex);
        }
    }

    private void CancelActiveBatch()
    {
        if (_activeBatchCts == null) return;
        try
        {
            _activeBatchCts.Cancel();
            _activeBatchCts.Dispose();
        }
        catch (ObjectDisposedException) { /* Safe to ignore */ }
        finally
        {
            _activeBatchCts = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // ReSharper disable once MethodHasAsyncOverload
        _lifecycleCts.Cancel();
        CancelActiveBatch();
        
        try { await _processingTask; } catch { /* Ignore task cancellation */ }
        
        _lifecycleCts.Dispose();
    }
}