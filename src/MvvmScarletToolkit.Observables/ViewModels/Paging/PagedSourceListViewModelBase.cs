using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public abstract class PagedSourceListViewModelBase<TViewModel> : SourceListViewModelBase<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        private readonly IPagedDataProvider<TViewModel> _pagedDataProvider;

        protected readonly IObservableBusyStack BusyStack;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IScarletCommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;

        private TViewModel? _selectedItem;
        [Bindable(true, BindingDirection.TwoWay)]
        public virtual TViewModel? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public ObservableCollection<TViewModel> SelectedItems { get; }

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetProperty(ref _isBusy, value); }
        }

        private int _totalPageCount;
        [Bindable(true, BindingDirection.TwoWay)]
        public int TotalPageCount
        {
            get { return _totalPageCount; }
            protected set { SetProperty(ref _totalPageCount, value); }
        }

        private int _pageSize;
        [Bindable(true, BindingDirection.TwoWay)]
        public int PageSize
        {
            get { return _pageSize; }
            set { SetProperty(ref _pageSize, value); }
        }

        private int _currentPage;
        [Bindable(true, BindingDirection.TwoWay)]
        public int CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand NextCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand PreviousCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand FirstCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand LastCommand { get; }

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

        private readonly ConcurrentCommandBase _loadCommand;
        /// <summary>
        /// Executes <see cref="Refresh"/> unless, <see cref="Load"/> has already been called
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand LoadCommand => _loadCommand;

        private readonly ConcurrentCommandBase _refreshCommand;
        /// <summary>
        /// Clears <see cref="Items"/> to add new instances
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RefreshCommand => _refreshCommand;

        private readonly ConcurrentCommandBase _unloadCommand;
        /// <summary>
        /// Undoes <see cref="Load"/>. Clears all instances by default.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand UnloadCommand => _unloadCommand;

        protected PagedSourceListViewModelBase(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext, Func<TViewModel, string> selector, IPagedDataProvider<TViewModel> pagedDataProvider)
            : base(synchronizationContext, selector)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.CommandManager));

            _pagedDataProvider = pagedDataProvider ?? throw new ArgumentNullException(nameof(pagedDataProvider));

            SelectedItems = new ObservableCollection<TViewModel>();
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);
            PageSize = 50;

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

            NextCommand = commandBuilder
                .Create(Next, CanNext)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            PreviousCommand = commandBuilder
                .Create(Previous, CanPrevious)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            FirstCommand = commandBuilder
                .Create(First, CanFirst)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            LastCommand = commandBuilder
                .Create(Last, CanLast)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();
        }

        public async Task Refresh(CancellationToken token)
        {
            var results = await _pagedDataProvider.Get(CurrentPage * PageSize, PageSize, token).ConfigureAwait(false);

            AddOrUpdateMany(results);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            return Refresh(token);
        }

        public async Task Load(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await LoadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
            }
        }

        public virtual bool CanLoad()
        {
            return !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected virtual async Task UnloadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => Clear()).ConfigureAwait(false);
            }
        }

        public async Task Unload(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await UnloadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
            }
        }

        public virtual bool CanUnload()
        {
            return !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        public virtual bool CanRefresh()
        {
            return IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        private int GetNext()
        {
            var next = CurrentPage + 1;
            var last = TotalPageCount;

            return next < last ? next : last;
        }

        private Task Next(CancellationToken token)
        {
            return Set(GetNext(), token);
        }

        private bool CanNext()
        {
            return CanRefresh() && CurrentPage + 1 < TotalPageCount;
        }

        private int GetPrevious()
        {
            const int first = 1;
            var previous = CurrentPage - 1;

            return previous > first ? previous : first;
        }

        private Task Previous(CancellationToken token)
        {
            return Set(GetPrevious(), token);
        }

        private bool CanPrevious()
        {
            return CanRefresh() && CurrentPage - 1 > 0;
        }

        private int GetFirst()
        {
            return 1;
        }

        private Task First(CancellationToken token)
        {
            return Set(GetFirst(), token);
        }

        private bool CanFirst()
        {
            return CanRefresh() && CurrentPage > 1;
        }

        private int GetLast()
        {
            return TotalPageCount;
        }

        private Task Last(CancellationToken token)
        {
            return Set(GetLast(), token);
        }

        private bool CanLast()
        {
            return CanRefresh() && CurrentPage + 1 < TotalPageCount;
        }

        private async Task Set(int index, CancellationToken token)
        {
            await Dispatcher.Invoke(() => CurrentPage = index, token).ConfigureAwait(false);
            await Refresh(token).ConfigureAwait(false);
        }
    }
}
