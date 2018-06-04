using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed class ObservableCircularBuffer<T> : ObservableObject, IEnumerable<T>, INotifyCollectionChanged
    {
        private readonly ObservableCollection<T> _buffer;
        private int _start;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private int _capacity;
        public int Capacity
        {
            get { return _capacity; }
            set { SetValue(ref _capacity, value); }
        }

        private int _size;
        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size
        {
            get { return _size; }
            private set { SetValue(ref _size, value); }
        }

        public bool IsFull
        {
            get { return _size == Capacity; }
        }

        public ObservableCircularBuffer(int capacity)
            : this(capacity, new T[] { })
        {
        }

        public ObservableCircularBuffer(int capacity, IEnumerable<T> items)
        {
            if (capacity < 1)
                throw new ArgumentException("Circular buffer cannot have negative or zero capacity.", "capacity");

            if (items == null)
                throw new ArgumentNullException("items");

            Size = items.Count();

            if (Size > capacity)
                throw new ArgumentException("Too many items to fit circular buffer", "items");

            _start = 0;
            _buffer = new ObservableCollection<T>(items);
            _buffer.CollectionChanged += (o, e) => OnCollectionChanged(e);

            Capacity = capacity;
        }

        public void Push(T item)
        {
            if (IsFull)
            {
                for (var i = 1; i < Capacity - 1; i++)
                    _buffer.Move(i, i + 1);

                Decrement(ref _start);
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer.Add(item);
                ++Size;

                if (IsFull)
                    OnPropertyChanged(nameof(IsFull));
            }
        }

        private void Decrement(ref int index)
        {
            if (index == 0)
                index = Capacity;

            index--;
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
