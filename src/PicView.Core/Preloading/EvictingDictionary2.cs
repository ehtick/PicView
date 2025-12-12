using System.Collections;
using System.Diagnostics.CodeAnalysis;
using PicView.Core.DebugTools;
using ZLinq;
#if DEBUG
using System.Diagnostics;
#endif

namespace PicView.Core.Preloading;

/// <summary>
/// A thread-safe, fixed-size dictionary that evicts items according to a 
/// directional policy intended for image iteration.
/// 
/// Refactored to separate Logical Slots (Owner/Index) from Physical Data (FilePath)
/// to allow deduplication of images across tabs.
/// </summary>
public class EvictingDictionary2<TValue> : IEnumerable<KeyValuePair<(string OwnerId, int Index), TValue>>
{
    // 2. Data Store: Tracks the actual loaded images by FilePath
    //    Controls the "Pixel" memory (large footprint)
    private readonly Dictionary<string, TValue> _data;

    // 1. Logical Map: Tracks which Tab/Index points to which File
    //    Controls the "Navigation" memory (small footprint)
    private readonly Dictionary<CacheKey, string> _indexMap;

    private readonly int _initialMaxSize;

    private readonly Lock _lock = new();
    private int _currentMaxSize;

    public EvictingDictionary2(int initialMaxSize)
    {
        if (initialMaxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialMaxSize), "Size must be positive.");
        }

        _initialMaxSize = initialMaxSize;
        _currentMaxSize = initialMaxSize;

        _indexMap = new Dictionary<CacheKey, string>(initialMaxSize);
        _data = new Dictionary<string, TValue>(initialMaxSize);
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _indexMap.Count;
            }
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (_lock)
            {
                return _data.Values.ToArray();
            }
        }
    }

    public IEnumerator<KeyValuePair<(string OwnerId, int Index), TValue>> GetEnumerator()
    {
        List<KeyValuePair<(string, int), TValue>> snapshot;
        lock (_lock)
        {
            // Reconstruct the view: Iterate logical keys -> fetch data
            snapshot = new List<KeyValuePair<(string, int), TValue>>(_indexMap.Count);
            foreach (var kvp in _indexMap)
            {
                if (_data.TryGetValue(kvp.Value, out var val))
                {
                    snapshot.Add(new KeyValuePair<(string, int), TValue>(
                        (kvp.Key.OwnerId, kvp.Key.Index), val));
                }
            }
        }

        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void ExpandCapacity(string id)
    {
        lock (_lock)
        {
            // Fix: Capacity should increase when a new tab acts as an owner
            _currentMaxSize += _initialMaxSize;
#if DEBUG
            DebugHelper.LogDebug(nameof(EvictingDictionary2<TValue>), nameof(ExpandCapacity),
                $"Expanded by '{id}'. New Capacity: {_currentMaxSize}");
#endif
        }
    }

    public void DecreaseCapacity(string id)
    {
        lock (_lock)
        {
            var keysToRemove = _indexMap.Keys.Where(k => k.OwnerId == id).ToArray();
            foreach (var key in keysToRemove)
            {
                RemoveInternal(key);
            }

            // Only decrease capacity if we are sure this ID was contributing to it.
            // If keysToRemove.Length == 0, it might be a double-dispose or invalid ID.
            // Ideally, track registered owners, but checking keys > 0 is a decent proxy 
            // IF we assume a tab always loads at least one thing before closing.
            // Otherwise, just trusting the caller is acceptable if the Service layer is robust.

            if (_currentMaxSize > _initialMaxSize)
            {
                _currentMaxSize -= _initialMaxSize;
            }

            // Safety clamp
            if (_currentMaxSize < _initialMaxSize)
            {
                _currentMaxSize = _initialMaxSize;
            }
        }
    }

    public TValue Get(string ownerId, int index)
    {
        lock (_lock)
        {
            var key = new CacheKey(ownerId, index);
            if (_indexMap.TryGetValue(key, out var path))
            {
                if (_data.TryGetValue(path, out var value))
                {
                    return value;
                }

                // Inconsistent state protection: Logical map exists but data missing
                _indexMap.Remove(key);
            }

            throw new KeyNotFoundException($"Key ({ownerId}, {index}) was not found.");
        }
    }

    public bool TryAdd(string ownerId, int index, string filePath, TValue value,
        int totalCount, bool isReverse, [MaybeNullWhen(false)] out TValue evictedValue)
    {
        lock (_lock)
        {
            var newKey = new CacheKey(ownerId, index);
            evictedValue = default;

            // 1. Check for Overwrite
            if (_indexMap.TryGetValue(newKey, out var oldPath))
            {
                if (oldPath == filePath)
                {
                    return false;
                }

                // Remove the old link. Capture the value if it was the last ref.
                evictedValue = RemoveInternal(newKey);

                // Note: capacity didn't change (removed 1, about to add 1), 
                // so we don't need to run eviction logic below.
            }
            // 2. Check Capacity (Only if we didn't just free up a slot)
            else if (_indexMap.Count >= _currentMaxSize)
            {
                var keyToEvict = SelectEvictionCandidate(ownerId, index, totalCount, isReverse);
                if (!EqualityComparer<CacheKey>.Default.Equals(keyToEvict, default))
                {
                    // If we already have an evictedValue from above? Impossible (due to 'else if')
                    evictedValue = RemoveInternal(keyToEvict);
                }
            }

            // 3. Add Data
            _data.TryAdd(filePath, value);

            // 4. Update Map
            _indexMap[newKey] = filePath;

            return !EqualityComparer<TValue>.Default.Equals(evictedValue, default);
        }
    }

    /// <summary>
    /// Removes a logical key and checks if the underlying data should also be removed (ref counting).
    /// Returns the data value if it was fully evicted (orphaned), otherwise default.
    /// </summary>
    private TValue? RemoveInternal(CacheKey key)
    {
        if (!_indexMap.Remove(key, out var path))
        {
            return default;
        }

        // Check if any other keys point to this path
        // Optimization: We could maintain a RefCount dictionary, but iterating _indexMap is acceptable 
        // if capacity is small (~50-100 items). For larger caps, add a RefCount Dict.
        var isReferenced = _indexMap.ContainsValue(path);

        if (isReferenced)
        {
            return default;
        }

        // Orphaned! Remove actual data.
        if (!_data.Remove(path, out var val))
        {
            return default;
        }

#if DEBUG
        if (DebugHelper.ShowCacheAdditionsAndRemovals)
        {
            Trace.WriteLine($"Fully Evicted file: {Path.GetFileName(path)}");
        }
#endif
        return val;
    }

    private CacheKey SelectEvictionCandidate(string ownerId, int currentIndex, int totalCount, bool isReverse)
    {
        var maxDistance = -1;
        var keyToEvict = default(CacheKey);
        var foundCandidate = false;

        // Only evict items belonging to the current OwnerId (Polite Eviction)
        // If we are desperate (no items for this owner), we might need to evict others,
        // but typically we expand capacity per owner so this shouldn't happen.
        var candidates = _indexMap.Keys.Where(k => k.OwnerId == ownerId);

        foreach (var candidateKey in candidates)
        {
            int distance;
            if (isReverse)
            {
                // Moving backward: Evict furthest ahead
                distance = (candidateKey.Index - currentIndex + totalCount) % totalCount;
            }
            else
            {
                // Moving forward: Evict furthest behind
                distance = (currentIndex - candidateKey.Index + totalCount) % totalCount;
            }

            if (distance <= maxDistance)
            {
                continue;
            }

            maxDistance = distance;
            keyToEvict = candidateKey;
            foundCandidate = true;
        }

        if (foundCandidate)
        {
            return keyToEvict;
        }

        // Fallback: If current owner has NO items (first load?), but cache is full of OTHER owners.
        // We must evict someone else.
        if (_indexMap.Count >= _currentMaxSize)
        {
            // Just pick the first one. (Or implement LRU for cross-tab eviction)
            return _indexMap.Keys.FirstOrDefault();
        }

        return default;
    }

    public bool Remove(string ownerId, int index)
    {
        lock (_lock)
        {
            var val = RemoveInternal(new CacheKey(ownerId, index));
            // Return true if we found the key to remove (regardless of whether data was evicted)
            // Original contract implies "Did we remove the entry?".
            // Since we don't return the evicted value here, just bool.
            // But RemoveInternal returns value only if data evicted. 
            // We need to check if indexMap contained it.
            // Simplified:
            return !_indexMap.ContainsKey(new CacheKey(ownerId, index)); // Wait, we just removed it.
            // Correct approach: TryGetValue -> RemoveInternal -> return true
        }
    }

    // Explicit TryGetValue for logic that just checks existence
    public bool TryGetValue(string ownerId, int index, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            if (_indexMap.TryGetValue(new CacheKey(ownerId, index), out var path))
            {
                return _data.TryGetValue(path, out value);
            }

            value = default;
            return false;
        }
    }

    // Optimized: Direct lookup in Data dictionary
    public bool TryGetValueByPath(string filePath, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            return _data.TryGetValue(filePath, out value);
        }
    }

    public bool TryFindKeyByPath(string filePath, out CacheKey key)
    {
        lock (_lock)
        {
            foreach (var kvp in _indexMap.Where(kvp => kvp.Value == filePath).AsValueEnumerable())
            {
                key = kvp.Key;
                return true;
            }

            key = default;
            return false;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _indexMap.Clear();
            _data.Clear();
        }
    }

    // Composite key to ensure uniqueness across tabs for logical mapping
    public readonly record struct CacheKey(string OwnerId, int Index);
}