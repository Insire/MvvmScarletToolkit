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

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelListBase<T> : ObservableObject
        where T : class, INotifyPropertyChanged
    {
        private readonly ObservableCollection<T> _items;

        protected readonly ObservableBusyStack BusyStack;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly ICommandManager CommandManager;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        private T _selectedItem;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual T SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public T this[int index]
        {
            get { return _items[index]; }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand RemoveRangeCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand RemoveCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand ClearCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand LoadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand RefreshCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand UnloadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public IReadOnlyCollection<T> Items { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public int Count => Items.Count;

        protected ViewModelListBase(IScarletDispatcher dispatcher, ICommandManager commandManager)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));

            _items = new ObservableCollection<T>();

            Items = new ReadOnlyObservableCollection<T>(_items);

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);

            RemoveCommand = AsyncCommand.Create<T>(Remove, CanRemove, commandManager).AsSequential();
            RemoveRangeCommand = AsyncCommand.Create<IList>(RemoveRange, CanRemoveRange, commandManager).AsSequential();
            ClearCommand = AsyncCommand.Create(Clear, CanClear, commandManager).AsSequential();

            LoadCommand = AsyncCommand.Create(LoadInternal, CanLoad, commandManager).AsSequential();
            RefreshCommand = AsyncCommand.Create(RefreshInternal, CanRefresh, commandManager);
            UnloadCommand = AsyncCommand.Create(UnloadInternalAsync, CanUnload, commandManager).AsSequential();
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
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count))).ConfigureAwait(false);
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
                await items.ForEachAsync(Add).ConfigureAwait(false);
            }
        }

        public virtual async Task Remove(T item)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Remove(item)).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count))).ConfigureAwait(false);
            }
        }

        public virtual async Task RemoveRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                await items.ForEachAsync(Remove).ConfigureAwait(false);
            }
        }

        private async Task RemoveRange(IList items)
        {
            await RemoveRange(items?.Cast<T>()).ConfigureAwait(false);
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

        public async Task Clear()
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Clear()).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count))).ConfigureAwait(false);
            }
        }

        protected bool CanClear()
        {
            return _items.Count > 0;
        }

        protected abstract Task LoadInternal(CancellationToken token);

        protected virtual bool CanLoad()
        {
            return !IsLoaded;
        }

        protected abstract Task UnloadInternalAsync();

        protected virtual bool CanUnload()
        {
            return IsLoaded;
        }

        protected abstract Task RefreshInternal(CancellationToken token);

        protected virtual bool CanRefresh()
        {
            return IsLoaded;
        }
    }
}
