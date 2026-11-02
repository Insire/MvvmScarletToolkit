using Microsoft.Extensions.Caching.Memory;

namespace MvvmScarletToolkit.ImageLoading.Caches
{
    /// <summary>
    /// Caps how many entries a cache keeps in an <see cref="IMemoryCache"/>, removing the least recently used ones first.
    /// The <see cref="IMemoryCache"/> is usually shared with the rest of the application, so its own <see cref="MemoryCacheOptions.SizeLimit"/>
    /// can't express a limit for a single cache. Only the entries added through <see cref="Add"/> are counted and removed.
    /// </summary>
    internal sealed class MemoryCacheEntryLimit
    {
        private readonly object _syncRoot = new();
        private readonly LinkedList<object> _recentlyUsed = new();
        private readonly Dictionary<object, LinkedListNode<object>> _nodes = new();
        private readonly IMemoryCache _memoryCache;
        private readonly int _maxEntries;

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _nodes.Count;
                }
            }
        }

        public MemoryCacheEntryLimit(IMemoryCache memoryCache, int maxEntries)
        {
            _memoryCache = memoryCache;
            _maxEntries = maxEntries;
        }

        /// <summary>
        /// Tracks <paramref name="entry"/> before it gets committed, and removes the least recently used entries above the limit.
        /// </summary>
        public void Add(ICacheEntry entry)
        {
            List<object>? removedKeys = null;

            lock (_syncRoot)
            {
                if (_nodes.TryGetValue(entry.Key, out var replacedNode))
                {
                    _recentlyUsed.Remove(replacedNode);
                }

                // a new node for every entry, so that the eviction of a replaced entry can't untrack its successor
                var node = _recentlyUsed.AddFirst(entry.Key);
                _nodes[entry.Key] = node;
                entry.RegisterPostEvictionCallback(OnEviction, node);

                while (_recentlyUsed.Count > _maxEntries)
                {
                    var oldest = _recentlyUsed.Last!;
                    _recentlyUsed.RemoveLast();
                    _nodes.Remove(oldest.Value);

                    (removedKeys ??= []).Add(oldest.Value);
                }
            }

            // outside the lock, removing raises eviction callbacks that take it as well
            if (removedKeys is not null)
            {
                foreach (var key in removedKeys)
                {
                    _memoryCache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Marks the entry for <paramref name="key"/> as most recently used.
        /// </summary>
        public void Touch(object key)
        {
            lock (_syncRoot)
            {
                if (_nodes.TryGetValue(key, out var node) && node != _recentlyUsed.First)
                {
                    _recentlyUsed.Remove(node);
                    _recentlyUsed.AddFirst(node);
                }
            }
        }

        private void OnEviction(object key, object? value, EvictionReason reason, object? state)
        {
            lock (_syncRoot)
            {
                // the key might already belong to a newer entry, that one stays tracked
                if (_nodes.TryGetValue(key, out var node) && ReferenceEquals(node, state))
                {
                    _recentlyUsed.Remove(node);
                    _nodes.Remove(key);
                }
            }
        }
    }
}
