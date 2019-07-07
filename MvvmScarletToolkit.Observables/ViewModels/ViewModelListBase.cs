using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Collection ViewModelBase that provides async modification
    /// </summary>
    public abstract class ViewModelListBase<TViewModel> : INotifyPropertyChanged, IDisposable
        where TViewModel : class, INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _propertyChangedCache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        protected readonly ObservableCollection<TViewModel> _items;
        protected readonly ObservableBusyStack BusyStack;
        protected readonly ICommandBuilder CommandBuilder;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletMessenger Messenger;
        protected readonly IExitService Exit;
        protected readonly IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;
        private bool _disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        private TViewModel _selectedItem;
        [Bindable(true, BindingDirection.TwoWay)]
        public virtual TViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value, OnChanged: OnSelectionChanged, OnChanging: OnSelectionChanging); }
        }

        private IList _selectedItems;
        [Bindable(true, BindingDirection.TwoWay)]
        public virtual IList SelectedItems
        {
            get { return _selectedItems; }
            set { SetValue(ref _selectedItems, value, OnChanged: OnSelectionsChanged, OnChanging: OnSelectionsChanging); }
        }

        public TViewModel this[int index]
        {
            get { return _items[index]; }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public ReadOnlyObservableCollection<TViewModel> Items { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public int Count => Items.Count;

        protected ViewModelListBase(ICommandBuilder commandBuilder)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(ICommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(ICommandBuilder.CommandManager));
            Messenger = commandBuilder.Messenger ?? throw new ArgumentNullException(nameof(ICommandBuilder.Messenger));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(ICommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(ICommandBuilder.WeakEventManager));

            _items = new ObservableCollection<TViewModel>();

            SelectedItems = new ObservableCollection<TViewModel>();
            Items = new ReadOnlyObservableCollection<TViewModel>(_items);
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);
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

        public async Task Clear(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Clear(), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count)), token).ConfigureAwait(false);
            }
        }

        public bool CanClear()
        {
            return _items.Count > 0 && !IsBusy;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, _propertyChangedCache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
        }

        protected Task Remove()
        {
            return Remove(SelectedItem);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (disposing)
            {
                BusyStack.Dispose();
            }
        }

        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void OnSelectionChanged()
        {
            ThrowIfDisposed();
            Messenger.Publish(new ViewModelListBaseSelectionChanged<TViewModel>(this, SelectedItem));
        }

        private void OnSelectionChanging()
        {
            ThrowIfDisposed();
            Messenger.Publish(new ViewModelListBaseSelectionChanging<TViewModel>(this, SelectedItem));
        }

        private void OnSelectionsChanged()
        {
            ThrowIfDisposed();
            Messenger.Publish(new ViewModelListBaseSelectionsChanged<TViewModel>(this, SelectedItems?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>()));
        }

        private void OnSelectionsChanging()
        {
            ThrowIfDisposed();
            Messenger.Publish(new ViewModelListBaseSelectionsChanging<TViewModel>(this, SelectedItems?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>()));
        }
    }
}
