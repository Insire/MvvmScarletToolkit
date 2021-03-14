using Microsoft.Toolkit.Mvvm.Messaging;
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
    /// <summary>
    /// Collection ViewModelBase wrapping around an <see cref="ObservableCollection"/> that provides methods for threadsafe modification via the dispatcher
    /// </summary>
    public abstract class ViewModelListBase<TViewModel> : ViewModelBase
        where TViewModel : class, INotifyPropertyChanged
    {
        protected readonly ObservableCollection<TViewModel> _items;

        private TViewModel? _selectedItem;
        [Bindable(true, BindingDirection.TwoWay)]
        public virtual TViewModel? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public TViewModel this[int index]
        {
            get { return _items[index]; }
        }

        /// <summary>
        /// <para>Readonly collection of all the entries managed by this instance</para>
        /// <para>Using <see cref="ICollection{TViewModel}.Add"/> on this, will result in a <see cref="NotSupportedException"/></para>
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public ReadOnlyObservableCollection<TViewModel> Items { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ObservableCollection<TViewModel> SelectedItems { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public int Count => Items.Count;

        [Bindable(true, BindingDirection.OneWay)]
        public bool HasItems => Items.Count > 0;

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand ClearCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RemoveRangeCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RemoveCommand { get; }

        protected ViewModelListBase(in IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _items = new ObservableCollection<TViewModel>();
            SelectedItems = new ObservableCollection<TViewModel>();

            Items = new ReadOnlyObservableCollection<TViewModel>(_items);

            RemoveCommand = commandBuilder
                .Create(Remove, CanRemove)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            RemoveRangeCommand = commandBuilder
                .Create(RemoveRange, CanRemoveRange)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            ClearCommand = commandBuilder
                .Create(() => Clear(), CanClear)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            PropertyChanging += ViewModelListBase_PropertyChanging;
            PropertyChanged += ViewModelListBase_PropertyChanged;
        }

        private void ViewModelListBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModelListBase<TViewModel>.SelectedItem):
                    OnSelectionChanged();
                    break;

                case nameof(ViewModelListBase<TViewModel>.SelectedItems):
                    OnSelectionsChanged();
                    break;
            }
        }

        private void ViewModelListBase_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModelListBase<TViewModel>.SelectedItem):
                    OnSelectionChanging();
                    break;

                case nameof(ViewModelListBase<TViewModel>.SelectedItems):
                    OnSelectionsChanging();
                    break;
            }
        }

        /// <summary>
        ///<para>
        /// This method exists for usability reasons, so that one can mdofiy the internal collection from within a constructor where Tasks can't/should't be run.
        /// </para>
        /// <para>
        /// Modify the internal collection synchronously. No checks are being performed here. This method is not threadsafe.
        /// </para>
        /// </summary>
        /// <param name="viewModel">the viewmodel instance to be added</param>
        protected void AddUnchecked(TViewModel viewModel)
        {
            _items.Add(viewModel);
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

        public virtual async Task Add(TViewModel item, CancellationToken token)
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
                await Dispatcher.Invoke(() => _items.Add(item), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count)), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(HasItems)), token).ConfigureAwait(false);
            }
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

        public Task Remove(TViewModel? item)
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

        public virtual async Task Remove(TViewModel item, CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Remove(item), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count)), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(HasItems)), token).ConfigureAwait(false);
            }
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

        public Task Clear()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            return Clear(CancellationToken.None);
        }

        public virtual async Task Clear(CancellationToken token)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _items.Clear(), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(Count)), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => OnPropertyChanged(nameof(HasItems)), token).ConfigureAwait(false);
            }
        }

        public virtual bool CanClear()
        {
            return !IsDisposed
                && HasItems
                && !IsBusy;
        }

        protected Task Remove()
        {
            return Remove(SelectedItem);
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
                && items?.Any(p => _items.Contains(p)) == true;
        }

        protected bool CanRemoveRange(IList items)
        {
            return !IsDisposed
                && CanRemoveRange(items?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>());
        }

        protected virtual bool CanRemove(TViewModel? item)
        {
            if (item is null)
            {
                return false;
            }

            return !IsDisposed
                && CanClear()
                && !(item is null)
                && _items.Contains(item);
        }

        private bool CanRemoveRange()
        {
            return CanRemoveRange((IList)SelectedItems);
        }

        private bool CanRemove()
        {
            return CanRemove(SelectedItem);
        }

        private Task RemoveRange()
        {
            return RemoveRange((IList)SelectedItems);
        }

        private void OnSelectionChanged()
        {
            if (IsDisposed)
            {
                return;
            }

            Messenger.Send(new ViewModelListBaseSelectionChanged<TViewModel?>(this, SelectedItem));
        }

        private void OnSelectionChanging()
        {
            if (IsDisposed)
            {
                return;
            }

            Messenger.Send(new ViewModelListBaseSelectionChanging<TViewModel?>(this, SelectedItem));
        }

        private void OnSelectionsChanged()
        {
            if (IsDisposed)
            {
                return;
            }

            Messenger.Send(new ViewModelListBaseSelectionsChanged<TViewModel>(SelectedItems?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>()));
        }

        private void OnSelectionsChanging()
        {
            if (IsDisposed)
            {
                return;
            }

            Messenger.Send(new ViewModelListBaseSelectionsChanging<TViewModel>(SelectedItems?.Cast<TViewModel>() ?? Enumerable.Empty<TViewModel>()));
        }

        protected override async void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ViewModelListBase<TViewModel>));
            }

            if (disposing)
            {
                await Clear().ConfigureAwait(false);
            }

            base.Dispose(disposing);
        }
    }
}
