using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelListBase<T> : ObservableObject
        where T : class, INotifyPropertyChanged
    {
        private readonly ObservableCollection<T> _items;

        protected readonly ObservableBusyStack BusyStack;
        protected readonly IScarletDispatcher Dispatcher;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        private T _selectedItem;
        public virtual T SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public T this[int index]
        {
            get { return _items[index]; }
        }

        public virtual ICommand RemoveRangeCommand { get; }
        public virtual ICommand RemoveCommand { get; }
        public virtual ICommand ClearCommand { get; }

        public virtual IExtendedAsyncCommand LoadCommand { get; }

        public virtual IExtendedAsyncCommand RefreshCommand { get; }

        public virtual IExtendedAsyncCommand UnloadCommand { get; }

        public IReadOnlyCollection<T> Items { get; }

        public int Count => Items.Count;

        protected ViewModelListBase(IScarletDispatcher dispatcher)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _items = new ObservableCollection<T>();

            Items = new ReadOnlyObservableCollection<T>(_items);

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);

            RemoveCommand = new RelayCommand<T>(Remove, CanRemove);
            RemoveRangeCommand = new RelayCommand<IList>(RemoveRange, CanRemoveRange);
            ClearCommand = new RelayCommand(Clear, CanClear);

            LoadCommand = AsyncCommand.Create(LoadInternal, CanLoad);
            RefreshCommand = AsyncCommand.Create(RefreshInternal, CanRefresh);
            UnloadCommand = AsyncCommand.Create(UnloadInternalAsync, CanUnload);
        }

        public virtual async Task Add(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Add(item)).ConfigureAwait(false);

                OnPropertyChanged(nameof(Count));
            }
        }

        public virtual async Task AddRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                {
                    await Add(item).ConfigureAwait(false);
                }
            }
        }

        public virtual void Remove(T item)
        {
            using (BusyStack.GetToken())
            {
                _items.Remove(item);
            }

            OnPropertyChanged(nameof(Count));
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                {
                    Remove(item);
                }
            }
        }

        private void RemoveRange(IList items)
        {
            RemoveRange(items?.Cast<T>());
        }

        protected virtual bool CanRemove(T item)
        {
            return CanClear()
                && !(item is null)
                && _items.Contains(item);
        }

        protected virtual bool CanRemoveRange(IEnumerable<T> items)
        {
            return CanClear()
                && items?.Any(p => _items.Contains(p)) == true;
        }

        protected virtual bool CanRemoveRange(IList items)
        {
            return CanRemoveRange(items?.Cast<T>());
        }

        public void Clear()
        {
            using (BusyStack.GetToken())
            {
                _items.Clear();
            }

            OnPropertyChanged(nameof(Count));
        }

        protected bool CanClear()
        {
            return _items.Count > 0;
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                IsLoaded = true;
                return Task.CompletedTask;
            }
        }

        protected virtual bool CanLoad()
        {
            return !IsLoaded;
        }

        protected virtual Task UnloadInternalAsync()
        {
            using (BusyStack.GetToken())
            {
                IsLoaded = false;
                return Task.CompletedTask;
            }
        }

        protected virtual bool CanUnload()
        {
            return IsLoaded;
        }

        protected virtual Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                return Task.CompletedTask;
            }
        }

        protected virtual bool CanRefresh()
        {
            return IsLoaded;
        }
    }
}
