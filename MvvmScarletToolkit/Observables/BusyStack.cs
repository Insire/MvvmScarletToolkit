using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    /*
     * Usage: 
     * 
     * using System.Collections.Specialized;
     * 
     * ctor:
     * BusyStack = new BusyStack();
     * BusyStack.CollectionChanged += BusyStackChanged;
     * 
     * private void BusyStackChanged(object sender, NotifyCollectionChangedEventArgs e)
     * {
     *      IsBusy = BusyStack.Items.Count > 0;
     * }
     * 
     * using (var token = BusyStack.GetToken())
     * {
     *      run your operation here
     * }
     * 
     * IIsBusy.cs can help here 
     * 
     */
    /// <summary>
    /// BusyStack will handle notifying a viewmodel on if actions are pending
    /// </summary>
    public class BusyStack : ObservableObject, INotifyCollectionChanged
    {
        private ConcurrentBag<BusyToken> _items;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ConcurrentBag<BusyToken> Items
        {
            get { return _items; }
            set
            {
                if (EqualityComparer<ConcurrentBag<BusyToken>>.Default.Equals(value))
                    return;

                _items = value;
                OnPropertyChanged(nameof(Items));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, _items));
            }
        }

        public BusyStack()
        {
            Items = new ConcurrentBag<BusyToken>();
        }

        /// <summary>
        /// Tries to take an item from the stack and returns true if that action was successful
        /// </summary>
        /// <returns></returns>
        public bool Pull()
        {
            BusyToken token;
            var result = Items.TryTake(out token);

            if (result)
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, token));

            return result;
        }

        /// <summary>
        /// Adds a new <see cref="BusyToken"/> to the Stack
        /// </summary>
        public void Push(BusyToken token)
        {
            Items.Add(token);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, token));
        }

        /// <summary>
        /// Adds a new <see cref="BusyToken"/> to the Stack
        /// </summary>
        public void Push()
        {
            var token = GetToken();
            Items.Add(token);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, token));
        }

        public bool HasItems()
        {
            return Items.Count > 0;
        }

        public BusyToken GetToken(Task task, TaskScheduler scheduler)
        {
            var token = new BusyToken(this);
            token.TaskScheduler = scheduler;

            return token;
        }

        /// <summary>
        /// Returns a new <see cref="BusyToken"/> thats associated with <see cref="this"/> instance of a <see cref="BusyStack"/>
        /// </summary>
        /// <returns>a new <see cref="BusyToken"/></returns>
        public BusyToken GetToken()
        {
            return new BusyToken(this);
        }
    }
}
