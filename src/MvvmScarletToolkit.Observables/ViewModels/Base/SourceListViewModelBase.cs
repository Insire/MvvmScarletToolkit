using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit
{
    public abstract class SourceListViewModelBase<TViewModel> : ObservableObject, IDisposable
        where TViewModel : class
    {
        private readonly SourceCache<TViewModel, string> _sourceCache;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly IDisposable _subscription;

        private bool _disposedValue;

        public IObservableCollection<TViewModel> Items { get; }

        protected SourceListViewModelBase(SynchronizationContext synchronizationContext, Func<TViewModel, string> selector)
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

            _sourceCache = new SourceCache<TViewModel, string>(selector);
            Items = new ObservableCollectionExtended<TViewModel>();

            _subscription = Connect()
                .DistinctUntilChanged()
                .ObserveOn(_synchronizationContext)
                .Bind(Items)
                .DisposeMany()
                .Subscribe();
        }

        public abstract Task Refresh(CancellationToken token);

        protected void AddOrUpdateMany(IEnumerable<TViewModel> items)
        {
            _sourceCache.AddOrUpdate(items);
        }

        protected void AddOrUpdate(TViewModel item)
        {
            _sourceCache.AddOrUpdate(item);
        }

        public TViewModel? TryGet(string key)
        {
            var result = _sourceCache.Lookup(key);
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return null;
            }
        }

        protected void RemoveKey(string item)
        {
            _sourceCache.RemoveKey(item);
        }

        protected void Clear()
        {
            _sourceCache.Clear();
        }

        public IObservable<IChangeSet<TViewModel, string>> Connect()
        {
            return _sourceCache.Connect();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _subscription.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
