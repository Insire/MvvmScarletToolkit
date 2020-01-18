using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    public sealed class VirtualizingCollectionViewSource : ListCollectionView, IVirtualizingCollectionViewSource
    {
        public static VirtualizingCollectionViewSource Create<TViewModel>(IBusinessViewModelListBase<TViewModel> viewModelList, IScarletDispatcher dispatcher, ICache cache)
            where TViewModel : class, IVirtualizationViewModel
        {
            return new VirtualizingCollectionViewSource(viewModelList.Items, dispatcher, cache, new ViewModelAdapter<TViewModel>(viewModelList));
        }

        private readonly IScarletDispatcher _dispatcher;
        private readonly IVirtualizedListViewModel _viewModel;

        private readonly HashSet<object> _deferredItems;
        private readonly ICache _cache;

        private volatile bool _isDeferred;

        internal VirtualizingCollectionViewSource(IList list, IScarletDispatcher dispatcher, ICache cache, IVirtualizedListViewModel viewModel)
            : base(list)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _viewModel = viewModel as IVirtualizedListViewModel ?? throw new ArgumentException(nameof(viewModel));

            _deferredItems = new HashSet<object>();

            _cache.ItemRemoved += _cache_ItemRemoved;
        }

        private void _cache_ItemRemoved(object sender, CachedValueRemovedEventArgs e)
        {
            _viewModel.Deflate(e.Value, CancellationToken.None);
        }

        public void Dispose()
        {
            _cache.ItemRemoved -= _cache_ItemRemoved;
        }

        public override object GetItemAt(int index)
        {
            if (!_isDeferred)
            {
                _deferredItems.Clear();

                _dispatcher.Invoke(LoadDeferredItems);

                _isDeferred = true;
            }

            var item = base.GetItemAt(index);
            if (!_deferredItems.Contains(item))
                _deferredItems.Add(item);

            return item;
        }

        private async Task LoadDeferredItems()
        {
            var uniqueSet = new HashSet<object>();
            foreach (var item in _deferredItems)
            {
                var hashCode = item.GetHashCode();
                if (!_cache.Contains(hashCode.ToString()))
                    uniqueSet.Add(item);

                _cache.Add(hashCode.ToString(), item);
            }

            await _viewModel.Extend(uniqueSet, CancellationToken.None);
            _isDeferred = false;
        }
    }
}
