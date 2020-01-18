using MvvmScarletToolkit.Abstractions;
using System;
using System.Runtime.Caching;

namespace MvvmScarletToolkit.Samples
{
    public class DemoCache : ICache
    {
        private readonly MemoryCache _cache;
        private readonly CacheItemPolicy _policy;

        public event EventHandler<CachedValueRemovedEventArgs> ItemRemoved;

        public DemoCache()
        {
            _policy = new CacheItemPolicy
            {
                RemovedCallback = CacheEntryRemovedCallback
            };
            _cache = new MemoryCache("MvvmScarletToolkit.Samples");
        }

        private void CacheEntryRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            ItemRemoved.Invoke(this, new CachedValueRemovedEventArgs(arguments.CacheItem.Value));
        }

        public bool Add(string key, object data)
        {
            return _cache.Add(key, data, _policy);
        }

        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }
    }
}
