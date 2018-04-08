using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    internal class VirtualizationViewModel<TViewModel> : ObservableObject
        where TViewModel : class
    {
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set { SetValue(ref _isExpanded, value); }
        }

        private VirtualizationViewModelState _state;
        public VirtualizationViewModelState State // TODO add implementation details
        {
            get { return _state; }
            private set { SetValue(ref _state, value); }
        }

        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { SetValue(ref _viewModel, value); }
        }

        private ICommand _deflateCommand;
        public ICommand DeflateCommand
        {
            get { return _deflateCommand; }
            private set { SetValue(ref _deflateCommand, value); }
        }

        private IAsyncCommand _expandCommand;
        public IAsyncCommand ExpandCommand
        {
            get { return _expandCommand; }
            private set { SetValue(ref _expandCommand, value); }
        }

        public VirtualizationViewModel()
        {
            DeflateCommand = new RelayCommand(Deflate, CanDeflate);
            ExpandCommand = AsyncCommand.Create(Expand, CanExpand);
        }

        public async Task Expand()
        {
            if (ViewModel != null)
                return;

            //ViewModel = await _dataProvider.Get(Id).ConfigureAwait(true);
            IsExpanded = true;
        }

        public bool CanExpand()
        {
            return ViewModel == null && !IsExpanded;
        }

        public void Deflate()
        {
            ViewModel = null; // enables GC
            IsExpanded = false;
        }

        public bool CanDeflate()
        {
            return ViewModel != null && IsExpanded;
        }
    }

    public class VirtualizingCollectionViewSource : ListCollectionView
    {
        private readonly IVirtualizedListViewModel _sponsor;
        private readonly HashSet<object> _deferredItems;
        private readonly MemoryCache _cache;
        private readonly CacheItemPolicy _policy;

        private bool _isDeferred;

        public VirtualizingCollectionViewSource(MemoryCache cache, CacheItemPolicy policy, IList list)
            : base(list)
        {
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
            if (!_isDeferred)
            {
                _deferredItems.Clear();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)LoadDeferredItems);

                _isDeferred = true;
            }

            var item = base.GetItemAt(index);
            if (!_deferredItems.Contains(item))
                _deferredItems.Add(item);

            return item;
        }

        private void LoadDeferredItems()
        {
            var uniqueSet = new HashSet<object>();
            foreach (var item in _deferredItems)
            {
                var hashCode = item.GetHashCode();
                if (!_cache.Contains(hashCode.ToString()))
                    uniqueSet.Add(item);

                _cache.Add(new CacheItem(hashCode.ToString(), item), _policy);
            }

            _sponsor.ExtendItems(uniqueSet);
            _isDeferred = false;
        }
    }
}
