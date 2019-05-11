using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelListBase<TViewModel> : INotifyPropertyChanged
        where TViewModel : class, INotifyPropertyChanged
    {
        private readonly ObservableCollection<TViewModel> _items;

        protected readonly ObservableBusyStack BusyStack;
        protected readonly ICommandBuilder CommandBuilder;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IScarletCommandManager CommandManager;

        protected bool Disposed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private TViewModel _selectedItem;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual TViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public TViewModel this[int index]
        {
            get { return _items[index]; }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase RemoveRangeCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase RemoveCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase ClearCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase LoadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase RefreshCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase UnloadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public IReadOnlyCollection<TViewModel> Items { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public int Count => Items.Count;

        protected ViewModelListBase(ICommandBuilder commandBuilder)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(CommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(CommandBuilder.CommandManager));

            _items = new ObservableCollection<TViewModel>();

            Items = new ReadOnlyObservableCollection<TViewModel>(_items);
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);

            RemoveCommand = commandBuilder.Create<TViewModel>(Remove, CanRemove)
                                          .WithSingleExecution(CommandManager);

            RemoveRangeCommand = commandBuilder.Create<IList>(RemoveRange, CanRemoveRange)
                                               .WithSingleExecution(CommandManager);

            ClearCommand = commandBuilder.Create(Clear, CanClear)
                                         .WithSingleExecution(CommandManager);

            LoadCommand = commandBuilder.Create(LoadInternal, CanLoad)
                                        .WithSingleExecution(CommandManager);

            RefreshCommand = commandBuilder.Create(RefreshInternal, CanRefresh);

            UnloadCommand = commandBuilder.Create(UnloadInternal, CanUnload)
                                          .WithSingleExecution(CommandManager);
        }

        public virtual async Task Add(TViewModel item)
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

        public virtual async Task AddRange(IEnumerable<TViewModel> items)
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

        public virtual async Task Remove(TViewModel item)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Remove(item)).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count))).ConfigureAwait(false);
            }
        }

        public virtual async Task RemoveRange(IEnumerable<TViewModel> items)
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
            using (BusyStack.GetToken())
            {
                await RemoveRange(items?.Cast<TViewModel>()).ConfigureAwait(false);
            }
        }

        protected virtual bool CanRemove(TViewModel item)
        {
            return CanClear()
                && !(item is null)
                && _items.Contains(item);
        }

        protected virtual bool CanRemoveRange(IEnumerable<TViewModel> items)
        {
            return CanClear()
                && items?.Any(p => _items.Contains(p)) == true;
        }

        protected virtual bool CanRemoveRange(IList items)
        {
            return CanRemoveRange(items?.Cast<TViewModel>());
        }

        public async Task Clear(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Clear()).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count))).ConfigureAwait(false);
            }
        }

        protected bool CanClear()
        {
            return _items.Count > 0 && !IsBusy;
        }

        protected virtual Task Load(CancellationToken token)
        {
            return Refresh(token);
        }

        protected virtual async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Load(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
            }
        }

        protected virtual bool CanLoad()
        {
            return !IsLoaded
                && !UnloadCommand.IsBusy
                && !LoadCommand.IsBusy
                && !RefreshCommand.IsBusy;
        }

        protected virtual async Task Unload(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Clear(token).ConfigureAwait(false);
            }
        }

        protected virtual async Task UnloadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Unload(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
            }
        }

        protected virtual bool CanUnload()
        {
            return IsLoaded
                && !UnloadCommand.IsBusy
                && !LoadCommand.IsBusy
                && !RefreshCommand.IsBusy;
        }

        protected abstract Task Refresh(CancellationToken token);

        protected virtual async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Clear(token).ConfigureAwait(false);
                await Refresh(token).ConfigureAwait(false);
            }
        }

        protected virtual bool CanRefresh()
        {
            return IsLoaded
                && !UnloadCommand.IsBusy
                && !LoadCommand.IsBusy
                && !RefreshCommand.IsBusy;
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, T value, Action OnChanged, [CallerMemberName]string propertyName = null)
        {
            return SetValue(ref field, value, null, OnChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, T value, Action OnChanging, Action OnChanged, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            OnChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            OnChanged?.Invoke();

            return true;
        }

        protected async void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            await Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))).ConfigureAwait(false);
        }
    }
}
