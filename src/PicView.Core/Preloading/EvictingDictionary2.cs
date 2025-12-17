using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace PicView.Core.Preloading;

/// <summary>
/// A thread-safe, capacity-constrained container responsible for storing loaded images and managing eviction.
/// <para>
/// This dictionary separates logical slots (Owner ID + Index) from physical data (File Paths) to allow 
/// deduplication across tabs. It enforces an eviction policy that removes items that are calculated 
/// to be too far away from the current viewing index of the specific tab owner.
/// </para>
/// </summary>
public class EvictingDictionary2<TValue> : IEnumerable<KeyValuePair<(string OwnerId, int Index), TValue>>
    where TValue : class
{
    private static readonly CacheKeyComparer Comparer = new();
    private readonly Dictionary<string, TValue> _data;

    // Storing Tuple (Path, Value) to avoid double-lookups on reads
    private readonly Dictionary<CacheKey, (string Path, TValue Value)> _indexMap;

    private readonly int _initialMaxSize;
    private readonly Lock _lock = new();
    private int _currentMaxSize;

    public EvictingDictionary2(int initialMaxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialMaxSize);

        _initialMaxSize = initialMaxSize;
        _currentMaxSize = initialMaxSize;

        _indexMap = new Dictionary<CacheKey, (string, TValue)>(initialMaxSize, Comparer);

        var pathComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        _data = new Dictionary<string, TValue>(initialMaxSize, pathComparer);
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
                if (_data.Count == 0)
                {
                    return Array.Empty<TValue>();
                }

                var arr = new TValue[_data.Count];
                _data.Values.CopyTo(arr, 0);
                return arr;
            }
        }
    }

    public IEnumerator<KeyValuePair<(string OwnerId, int Index), TValue>> GetEnumerator()
    {
        List<KeyValuePair<(string, int), TValue>> snapshot;
        lock (_lock)
        {
            snapshot = new List<KeyValuePair<(string, int), TValue>>(_indexMap.Count);
            foreach (var kvp in _indexMap)
            {
                snapshot.Add(new KeyValuePair<(string, int), TValue>(
                    (kvp.Key.OwnerId, kvp.Key.Index), kvp.Value.Value));
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
            _currentMaxSize += _initialMaxSize;
        }
    }

    public void DecreaseCapacity(string id)
    {
        lock (_lock)
        {
            // ALLOCATION FIX: Avoid LINQ
            List<CacheKey> keysToRemove = [];
            foreach (var key in _indexMap.Keys)
            {
                if (key.OwnerId == id)
                {
                    keysToRemove.Add(key);
                }
            }

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

    // ------------------------------------------------------------------------
    // HYBRID LOOKUP OPTIMIZATION
    // ------------------------------------------------------------------------

    /// <summary>
    /// FAST PATH: Uses standard string lookup to benefit from cached hash codes.
    /// Use this when you already have a string.
    /// </summary>
    public bool TryGetValueByPath(string filePath, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            return _data.TryGetValue(filePath, out value);
        }
    }

    /// <summary>
    /// ALLOCATION-FREE PATH: Uses Span lookup.
    /// Use this when your path is a slice of a larger buffer/span.
    /// </summary>
    public bool TryGetValueByPath(ReadOnlySpan<char> filePathSpan, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            var lookup = _data.GetAlternateLookup<ReadOnlySpan<char>>();
            return lookup.TryGetValue(filePathSpan, out value);
        }
    }

    public bool TryGetValue(string ownerId, int index, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_indexMap, new CacheKey(ownerId, index));
            if (!Unsafe.IsNullRef(ref entry))
            {
                value = entry.Value;
                return true;
            }

            value = default;
            return false;
        }
    }

    // ------------------------------------------------------------------------
    // ADD / REMOVE LOGIC
    // ------------------------------------------------------------------------

    public bool TryAdd(string ownerId, int index, string filePath, TValue value,
        int totalCount, bool isReverse, [MaybeNullWhen(false)] out TValue evictedValue)
    {
        lock (_lock)
        {
            var newKey = new CacheKey(ownerId, index);
            evictedValue = null;

            ref var existingEntry = ref CollectionsMarshal.GetValueRefOrNullRef(_indexMap, newKey);

            if (!Unsafe.IsNullRef(ref existingEntry))
            {
                if (existingEntry.Path == filePath)
                {
                    // IMPORTANT: ensure the slot points to the canonical instance
                    if (_data.TryGetValue(filePath, out var canonical) &&
                        !ReferenceEquals(existingEntry.Value, canonical))
                    {
                        existingEntry = (filePath, canonical);
                    }

                    return false; // no eviction, nothing else to do
                }

                evictedValue = RemoveInternal(newKey);
            }
            else if (_indexMap.Count >= _currentMaxSize)
            {
                var keyToEvict = SelectEvictionCandidate(ownerId, index, totalCount, isReverse);
                if (!EqualityComparer<CacheKey>.Default.Equals(keyToEvict, default))
                {
                    evictedValue = RemoveInternal(keyToEvict);
                }
            }

            // Canonicalize the value by filePath
            if (!_data.TryGetValue(filePath, out var storedValue))
            {
                _data[filePath] = value;
                storedValue = value;
            }
            // Always link the logical slot to the canonical stored value
            _indexMap[newKey] = (filePath, storedValue);

            // NOTE: if storedValue != value, value is redundant; if TValue can hold resources,
            // consider disposing it at the call-site or changing the API to return it.
            return evictedValue is not null;
        }
    }


    public bool TryRemove(string ownerId, int index, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            value = RemoveInternal(new CacheKey(ownerId, index));
            return value != null;
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

    public void Clear(string ownerId)
    {
        lock (_lock)
        {
            // ALLOCATION FIX: Manual iteration instead of LINQ
            List<CacheKey> toRemove = [];
            foreach (var k in _indexMap.Keys)
            {
                if (k.OwnerId == ownerId)
                {
                    toRemove.Add(k);
                }
            }

            foreach (var key in toRemove)
            {
                RemoveInternal(key);
            }
        }
    }

    /// <summary>
    /// Removes a logical key and checks if the underlying data should also be removed (ref counting).
    /// Returns the data value if it was fully evicted (orphaned), otherwise default.
    /// </summary>
    private TValue? RemoveInternal(CacheKey key)
    {
        if (!_indexMap.Remove(key, out var entry))
        {
            return null;
        }

        // Optimization: Check values manually to avoid LINQ/Lambdas
        var isReferenced = false;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var val in _indexMap.Values)
        {
            if (val.Path != entry.Path)
            {
                continue;
            }

            isReferenced = true;
            break;
        }

        if (isReferenced)
        {
            return null;
        }

        _data.Remove(entry.Path, out var valOut);
        return valOut;
    }

    private CacheKey SelectEvictionCandidate(string ownerId, int currentIndex, int totalCount, bool isReverse)
    {
        var maxDistance = -1;
        var keyToEvict = default(CacheKey);
        var foundCandidate = false;

        // ALLOCATION FIX: Replaced LINQ .Where() with manual foreach
        // This removes the Enumerator allocation on every eviction.
        foreach (var candidateKey in _indexMap.Keys)
        {
            if (candidateKey.OwnerId != ownerId)
            {
                continue;
            }

            int distance;
            if (isReverse)
            {
                distance = (candidateKey.Index - currentIndex + totalCount) % totalCount;
            }
            else
            {
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

        // Fallback: just remove first key if we are full and logic above failed
        if (_indexMap.Count < _currentMaxSize)
        {
            return default;
        }

        foreach (var k in _indexMap.Keys)
        {
            return k;
        }

        return default;
    }

    // -------------------------------------------------------------------------
    //  Helpers
    // -------------------------------------------------------------------------

    public readonly record struct CacheKey(string OwnerId, int Index);

    public readonly ref struct CacheKeyLookup(ReadOnlySpan<char> ownerIdSpan, int index)
    {
        public readonly ReadOnlySpan<char> OwnerIdSpan = ownerIdSpan;
        public readonly int Index = index;
    }

    private class CacheKeyComparer : IEqualityComparer<CacheKey>, IAlternateEqualityComparer<CacheKey, CacheKeyLookup>
    {
        public CacheKeyLookup Create(CacheKey alternate)
        {
            return new CacheKeyLookup(alternate.OwnerId.AsSpan(), alternate.Index);
        }

        public bool Equals(CacheKey key, CacheKeyLookup lookup)
        {
            return key.Index == lookup.Index && key.OwnerId.AsSpan().SequenceEqual(lookup.OwnerIdSpan);
        }

        public bool Equals(CacheKey x, CacheKey y)
        {
            return x.Index == y.Index && string.Equals(x.OwnerId, y.OwnerId, StringComparison.Ordinal);
        }

        public int GetHashCode(CacheKey obj)
        {
            return HashCode.Combine(obj.OwnerId, obj.Index);
        }

        public int GetHashCode(CacheKeyLookup lookup)
        {
            var hash = new HashCode();
            hash.Add(string.GetHashCode(lookup.OwnerIdSpan, StringComparison.Ordinal));
            hash.Add(lookup.Index);
            return hash.ToHashCode();
        }

        public CacheKey Create(CacheKeyLookup lookup)
        {
            return new CacheKey(lookup.OwnerIdSpan.ToString(), lookup.Index);
        }
    }
}