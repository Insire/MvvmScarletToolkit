using MvvmScarletToolkit.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelListBase<T> : ObservableObject
        where T : class, INotifyPropertyChanged
    {
        private readonly ObservableCollection<T> _items;
        protected readonly BusyStack BusyStack;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
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

        public IReadOnlyCollection<T> Items { get; }

        public int Count => Items.Count;

        protected ViewModelListBase()
        {
            _items = new ObservableCollection<T>();

            Items = new ReadOnlyObservableCollection<T>(_items);

            BusyStack = new BusyStack((hasItems) => IsBusy = hasItems);

            RemoveCommand = new RelayCommand<T>(Remove, CanRemove);
            RemoveRangeCommand = new RelayCommand<IList>(RemoveRange, CanRemoveRange);
            ClearCommand = new RelayCommand(Clear, CanClear);
        }

        protected ViewModelListBase(IEnumerable<T> items)
            : this()
        {
            AddRange(items);

            // initial Notification, so that the UI recognizes the value
            OnPropertyChanged(nameof(Count));
        }

        public virtual void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using (BusyStack.GetToken())
                _items.Add(item);

            OnPropertyChanged(nameof(Count));
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                    Add(item);
            }
        }

        protected virtual bool CanAdd()
        {
            return !(_items is null);
        }

        public virtual void Remove(T item)
        {
            using (BusyStack.GetToken())
                _items.Remove(item);

            OnPropertyChanged(nameof(Count));
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                    Remove(item);
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
                _items.Clear();

            OnPropertyChanged(nameof(Count));
        }

        protected bool CanClear()
        {
            return _items.Count > 0;
        }
    }
}
