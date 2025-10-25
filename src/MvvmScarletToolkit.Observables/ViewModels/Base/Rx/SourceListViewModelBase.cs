using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace MvvmScarletToolkit
{
    public abstract class SourceListViewModelBase<TViewModel> : ObservableObject, IDisposable
        where TViewModel : class
    {
        private readonly SourceCache<TViewModel, string> _sourceCache;
        private readonly IDisposable _subscription;

        private bool _disposedValue;

        protected int Threshold { get; set; } = 1;

        protected SynchronizationContext SynchronizationContext { get; }

        public IObservableCollection<TViewModel> Items { get; }

        protected SourceListViewModelBase(SynchronizationContext synchronizationContext, Func<TViewModel, string> selector)
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            SynchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

            _sourceCache = new SourceCache<TViewModel, string>(selector);
            Items = new ObservableCollectionExtended<TViewModel>();

            var observable = Connect()
                .ObserveOn(TaskPoolScheduler.Default)
                .DistinctUntilChanged();

            _subscription = observable
                .ObserveOn(SynchronizationContext)
                .Bind(Items)
                .DisposeMany()
                .Subscribe();
        }

        public IObservable<IChangeSet<TViewModel, string>> Connect()
        {
            return _sourceCache.Connect();
        }

        protected void AddOrUpdateMany(IEnumerable<TViewModel> viewModels)
        {
            _sourceCache.AddOrUpdate(viewModels);
        }

        protected TViewModel AddOrUpdate(TViewModel viewModel)
        {
            if (viewModel is null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            _sourceCache.AddOrUpdate(viewModel);

            return viewModel;
        }

        public TViewModel? TryGet(string key)
        {
            var viewModel = _sourceCache.Lookup(key);
            if (viewModel.HasValue)
            {
                return viewModel.Value;
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

        protected void Remove(TViewModel item)
        {
            _sourceCache.Remove(item);
        }

        protected void Clear()
        {
            _sourceCache.Clear();
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
