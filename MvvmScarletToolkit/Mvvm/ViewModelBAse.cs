using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public class ViewModelBase : ObservableObject
    {
        protected object _itemsLock;

        private bool _isBusy;
        /// <summary>
        /// Indicates if there is an operation running
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private BusyStack _busyStack;
        /// <summary>
        /// Provides IDisposable tokens for running async operations
        /// </summary>
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set { SetValue(ref _busyStack, value); }
        }
        
        private RangeObservableCollection<INotifyPropertyChanged> _items;
        /// <summary>
        /// Contains all the UI relevant Models and notifies about changes in the collection and inside the Models themself
        /// </summary>
        public RangeObservableCollection<INotifyPropertyChanged> Items
        {
            get { return _items; }
            private set { SetValue(ref _items, value); }
        }

        private ICollectionView _view;
        /// <summary>
        /// For grouping, sorting and filtering
        /// </summary>
        public ICollectionView View
        {
            get { return _view; }
            protected set { SetValue(ref _view, value); }
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public INotifyPropertyChanged this[int index]
        {
            get { return Items[index]; }
        }

        public ICommand RemoveCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        public ViewModelBase()
        {
            InitializeProperties();
            InitializeCommands();

            BindingOperations.EnableCollectionSynchronization(Items, _itemsLock);
        }

        private void InitializeProperties()
        {
            _itemsLock = new object();

            Items = new RangeObservableCollection<INotifyPropertyChanged>();
            Items.CollectionChanged += ItemsCollectionChanged;

            BusyStack = new BusyStack();
            BusyStack.CollectionChanged += BusyStackChanged;

            using (View.DeferRefresh())
                View = CollectionViewSource.GetDefaultView(Items);

            // initial Notification, so that UI recognizes the value
            OnPropertyChanged(nameof(Count));
        }

        private void InitializeCommands()
        {
            RemoveCommand = new RelayCommand<INotifyPropertyChanged>(Remove, CanRemove);
            ClearCommand = new RelayCommand(() => Clear(), CanClear);
        }

        public virtual void BusyStackChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsBusy = BusyStack.HasItems();
        }

        public virtual void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
        }

        public virtual void Add(INotifyPropertyChanged item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            using (BusyStack.GetToken())
                Items.Add(item);
        }

        public virtual void AddRange(IEnumerable<INotifyPropertyChanged> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            using (BusyStack.GetToken())
                Items.AddRange(items);
        }

        protected virtual bool CanAdd()
        {
            return Items != null;
        }

        public void Remove(INotifyPropertyChanged item)
        {
            using (BusyStack.GetToken())
                Items.Remove(item);
        }

        public void RemoveRange(IEnumerable<INotifyPropertyChanged> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            using (BusyStack.GetToken())
                Items.RemoveRange(items);
        }

        protected virtual bool CanRemove(INotifyPropertyChanged item)
        {
            return CanClear() && item != null && Items.Contains(item);
        }

        /// <summary>
        /// Checks if any of the submitted items can be removed
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        protected virtual bool CanRemoveRange(IEnumerable<INotifyPropertyChanged> items)
        {
            return CanClear() && items != null && items.Any(p => Items.Contains(p));
        }

        public void Clear()
        {
            using (BusyStack.GetToken())
                Items.Clear();
        }

        protected virtual bool CanClear()
        {
            return Items?.Any() == true;
        }
    }
}
