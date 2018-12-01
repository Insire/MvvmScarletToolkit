using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections;
using System.Runtime.Caching;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    public class VirtualizingCollectionViewSource : ListCollectionView
    {
        private readonly IScarletDispatcher _dispatcher;
        private readonly IVirtualizedListViewModel _sponsor;

        //private readonly HashSet<object> _deferredItems;
        private readonly MemoryCache _cache;

        private readonly CacheItemPolicy _policy;

        //private readonly bool _isDeferred;

        public VirtualizingCollectionViewSource(IScarletDispatcher dispatcher, MemoryCache cache, CacheItemPolicy policy, IList list)
            : base(list)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _sponsor = list as IVirtualizedListViewModel;
        }

        public void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            _sponsor.DeflateItem(arguments.CacheItem.Value);
        }

        public override object GetItemAt(int index)
        {
            throw new NotImplementedException();
            //if (!_isDeferred)
            //{
            //    _deferredItems.Clear();

            //    _dispatcher.Invoke(LoadDeferredItems);

            //    _isDeferred = true;
            //}

            //var item = base.GetItemAt(index);
            //if (!_deferredItems.Contains(item))
            //    _deferredItems.Add(item);

            //return item;
        }

        private void LoadDeferredItems()
        {
            throw new NotImplementedException();
            //var uniqueSet = new HashSet<object>();
            //foreach (var item in _deferredItems)
            //{
            //    var hashCode = item.GetHashCode();
            //    if (!_cache.Contains(hashCode.ToString()))
            //        uniqueSet.Add(item);

            //    _cache.Add(new CacheItem(hashCode.ToString(), item), _policy);
            //}

            //_sponsor.ExtendItems(uniqueSet);
            //_isDeferred = false;
        }
    }
}
