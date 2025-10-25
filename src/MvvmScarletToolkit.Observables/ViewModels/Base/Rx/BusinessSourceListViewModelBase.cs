using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public abstract class BusinessSourceListViewModelBase<TViewModel> : SourceListViewModelBase<TViewModel>, IVirtualizationViewModel
        where TViewModel : class, INotifyPropertyChanged
    {
        protected readonly IObservableBusyStack BusyStack;
        protected readonly IScarletCommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IExitService Exit;
        protected readonly IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;

        protected bool IsDisposed { get; private set; }

        [Bindable(true, BindingDirection.OneWay)]
        public ObservableCollection<TViewModel> SelectedItems { get; }

        private TViewModel? _selectedItem;
        [Bindable(true, BindingDirection.TwoWay)]
        public virtual TViewModel? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private bool _isLoaded;
        /// <summary>
        /// Indicates whether <see cref="Load"/> has been called already
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetProperty(ref _isLoaded, value); }
        }

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetProperty(ref _isBusy, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public bool HasItems => Items.Count > 0;

        private readonly ConcurrentCommandBase _loadCommand;
        /// <summary>
        /// Executes <see cref="Refresh"/> unless, <see cref="Load"/> has already been called
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand LoadCommand => _loadCommand;

        private readonly ConcurrentCommandBase _refreshCommand;
        /// <summary>
        /// Clears <see cref="Items"/> to add new instances
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand RefreshCommand => _refreshCommand;

        private readonly ConcurrentCommandBase _unloadCommand;
        /// <summary>
        /// Undoes <see cref="Load"/>. Clears all instances by default and resets state
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand UnloadCommand => _unloadCommand;

        private readonly ConcurrentCommandBase _removeRangeCommand;
        /// <summary>
        /// Removes all instances that are in <see cref="SelectedItems"/> from <see cref="Items"/>
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand RemoveRangeCommand => _removeRangeCommand;

        private readonly ConcurrentCommandBase _removeCommand;
        /// <summary>
        /// removes the instance in <see cref="SelectedItem"/> from <see cref="Items"/>
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand RemoveCommand => _removeCommand;

        private readonly ConcurrentCommandBase _clearCommand;
        /// <summary>
        /// Clears all instances and does not update state
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand ClearCommand => _clearCommand;

        protected BusinessSourceListViewModelBase(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext, Func<TViewModel, string> selector)
            : base(synchronizationContext, selector)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.CommandManager));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.WeakEventManager));

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);
            SelectedItems = new ObservableCollection<TViewModel>();

            _loadCommand = commandBuilder
                .Create(Load, CanLoad)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _refreshCommand = commandBuilder
                .Create(Refresh, CanRefresh)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _unloadCommand = commandBuilder
                .Create(Unload, CanUnload)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _removeCommand = commandBuilder
                .Create(Remove, CanRemove)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _removeRangeCommand = commandBuilder
                .Create(RemoveRange, CanRemoveRange)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _clearCommand = commandBuilder
                .Create(() => Clear(), CanClear)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            Exit.UnloadOnExit(this);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            return RefreshInternal(token);
        }

        public async Task Load(CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BusinessViewModelListBase<TViewModel>));
            }

            using (BusyStack.GetToken())
            {
                await LoadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
            }
        }

        public virtual bool CanLoad()
        {
            return !IsDisposed
                && !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected virtual Task UnloadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                Clear();
            }

            return Task.CompletedTask;
        }

        public async Task Unload(CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BusinessViewModelListBase<TViewModel>));
            }

            using (BusyStack.GetToken())
            {
                await UnloadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
            }
        }

        public virtual bool CanUnload()
        {
            return !IsDisposed
                && !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected abstract Task RefreshInternal(CancellationToken token);

        public async Task Refresh(CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BusinessViewModelListBase<TViewModel>));
            }

            using (BusyStack.GetToken())
            {
                base.Clear();
                await RefreshInternal(token).ConfigureAwait(false);
            }
        }

        public virtual bool CanRefresh()
        {
            return !IsDisposed
                && IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        public Task Add(TViewModel item)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (item is null)
            {
                return Task.CompletedTask;
            }

            return Add(item, CancellationToken.None);
        }

        public virtual Task Add(TViewModel item, CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            using (BusyStack.GetToken())
            {
                AddOrUpdate(item);
                OnPropertyChanged(nameof(HasItems));
            }

            return Task.CompletedTask;
        }

        public Task AddRange(IEnumerable<TViewModel> items)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            return AddRange(items, CancellationToken.None);
        }

        public virtual async Task AddRange(IEnumerable<TViewModel> items, CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                await items.ForEachAsync(Add, token).ConfigureAwait(false);
            }
        }

        protected Task Remove()
        {
            return Remove(SelectedItem);
        }

        private bool CanRemove()
        {
            return CanRemove(SelectedItem);
        }

        public new Task Remove(TViewModel? item)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (item is null)
            {
                return Task.CompletedTask;
            }

            return Remove(item, CancellationToken.None);
        }

        public virtual Task Remove(TViewModel item, CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (item is null)
            {
                return Task.CompletedTask;
            }

            using (BusyStack.GetToken())
            {
                base.Remove(item);
                OnPropertyChanged(nameof(HasItems));
            }

            return Task.CompletedTask;
        }

        private Task RemoveRange()
        {
            return RemoveRange((IList)SelectedItems);
        }

        public Task RemoveRange(IEnumerable<TViewModel> items)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            return RemoveRange(items, CancellationToken.None);
        }

        public virtual async Task RemoveRange(IEnumerable<TViewModel> items, CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                await items.ForEachAsync(Remove, token).ConfigureAwait(false);
            }
        }

        private async Task RemoveRange(IList items)
        {
            using (BusyStack.GetToken())
            {
                await RemoveRange(items?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>()).ConfigureAwait(false);
            }
        }

        protected virtual bool CanRemoveRange(IEnumerable<TViewModel> items)
        {
            return CanClear()
                && items?.Any(p => Items.Contains(p)) == true;
        }

        protected bool CanRemoveRange(IList items)
        {
            return !IsDisposed
                && CanRemoveRange(items?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>());
        }

        private bool CanRemoveRange()
        {
            return CanRemoveRange((IList)SelectedItems);
        }

        protected virtual bool CanRemove(TViewModel? item)
        {
            if (item is null)
            {
                return false;
            }

            return !IsDisposed
                && CanClear()
                && item is not null
                && Items.Contains(item);
        }

        public virtual bool CanClear()
        {
            return !IsDisposed
                && HasItems
                && !IsBusy;
        }

        public new Task Clear()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            base.Clear();

            return Task.CompletedTask;
        }

        protected override async void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            if (disposing)
            {
                if (IsLoaded && !IsBusy)
                {
                    await Unload(CancellationToken.None).ConfigureAwait(false);
                }

                BusyStack?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
