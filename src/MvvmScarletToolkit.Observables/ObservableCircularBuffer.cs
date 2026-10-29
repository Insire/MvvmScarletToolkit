using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Fixed size collection, that adds new entries at the beginning and removes the last entry, when it's full
    /// </summary>
    public sealed partial class ObservableCircularBuffer<T> : ObservableObject, IEnumerable<T>, INotifyCollectionChanged
    {
        private readonly ObservableCollection<T> _buffer;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFull))]
        public partial int Capacity { get; set; }

        private int _size;
        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size
        {
            get { return _size; }
            private set
            {
                if (SetProperty(ref _size, value))
                {
                    OnPropertyChanged(nameof(IsFull));
                }
            }
        }

        public bool IsFull => _size == Capacity;

        public T this[int index] => _buffer[index];

        public ObservableCircularBuffer(in int capacity)
            : this(capacity, [])
        {
        }

        public ObservableCircularBuffer(in int capacity, in IEnumerable<T> items)
        {
            if (capacity < 1)
            {
                throw new ArgumentException("Circular buffer cannot have negative or zero capacity.", nameof(capacity));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _buffer = new ObservableCollection<T>(items);
            _size = _buffer.Count;

            if (_size > capacity)
            {
                throw new ArgumentException("Too many items to fit circular buffer", nameof(items));
            }

            _buffer.CollectionChanged += (_, e) => OnCollectionChanged(e);
            Capacity = capacity;
        }

        /// <summary>
        /// Adds <paramref name="item"/> at the beginning of the buffer and removes the last entry, if the buffer is full
        /// </summary>
        public void Push(in T item)
        {
            if (IsFull)
            {
                _buffer.RemoveAt(_buffer.Count - 1);
                _buffer.Insert(0, item);
            }
            else
            {
                _buffer.Insert(0, item);
                Size = _buffer.Count;
            }
        }

        partial void OnCapacityChanging(int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Circular buffer cannot have negative or zero capacity.");
            }
        }

        partial void OnCapacityChanged(int value)
        {
            // the constructor sets the capacity after the buffer has been created
            if (_buffer is null)
            {
                return;
            }

            while (_buffer.Count > value)
            {
                _buffer.RemoveAt(_buffer.Count - 1);
            }

            Size = _buffer.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
