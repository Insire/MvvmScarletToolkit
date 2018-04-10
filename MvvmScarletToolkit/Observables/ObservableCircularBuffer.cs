﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// License: ---------------------------------------------------------------------------- "THE
// BEER-WARE LICENSE" (Revision 42): Joao Portela wrote this file.As long as you retain this notice
// you can do whatever you want with this stuff.If we meet some day, and you think this stuff is
// worth it, you can buy me a beer in return. Joao Portela ----------------------------------------------------------------------------

// based on source: https://github.com/joaoportela/CircullarBuffer-CSharp

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Circular buffer.
    ///
    /// When writting to a full buffer: PushBack -&gt; removes this[0] / Front() PushFront -&gt;
    /// removes this[Size-1] / Back()
    ///
    /// this implementation is inspired by
    /// http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html because I
    /// liked their interface.
    /// </summary>
    public sealed class ObservableCircularBuffer<T> : IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ObservableCollection<T> _buffer;
        private int _start;
        private int _end;

        public ObservableCircularBuffer(int capacity)
            : this(capacity, new T[] { })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCircularBuffer.CircularBuffer`1"/> class.
        /// </summary>
        /// <param name="capacity">Buffer capacity. Must be positive.</param>
        /// <param name="items">
        /// Items to fill buffer with. Items length must be less than capacity.
        /// Sugestion: use Skip(x).Take(y).ToArray() to build this argument from any enumerable.
        /// </param>
        public ObservableCircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("Circular buffer cannot have negative or zero capacity.", "capacity");

            if (items == null)
                throw new ArgumentNullException("items");

            if (items.Length > capacity)
                throw new ArgumentException("Too many items to fit circular buffer", "items");

            _buffer = new ObservableCollection<T>(items);
            _buffer.CollectionChanged += (o, e) => OnCollectionChanged(e);

            Capacity = capacity;
            Size = items.Length;

            _start = 0;
            _end = Size == capacity ? 0 : Size;
        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after maximum capacity is
        /// reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity { get; }

        public bool IsFull => Size == Capacity;

        public bool IsEmpty => Size == 0;

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size { get; private set; }
        public int Count { get; }
        public bool IsReadOnly { get; }

        /// <summary>
        /// Element at the front of the buffer - this[0].
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer.</returns>
        public T Front()
        {
            ThrowIfEmpty();
            return _buffer[_start];
        }

        /// <summary>
        /// Element at the back of the buffer - this[Size - 1].
        /// </summary>
        /// <returns>The value of the element of type T at the back of the buffer.</returns>
        public T Back()
        {
            ThrowIfEmpty();
            return _buffer[(_end != 0 ? _end : Size) - 1];
        }

        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));

                if (index >= Size)
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, Size));

                int actualIndex = InternalIndex(index);
                return _buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));

                if (index >= Size)
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, Size));

                int actualIndex = InternalIndex(index);
                _buffer[actualIndex] = value;
            }
        }

        /// <summary>
        /// Pushes a new element to the back of the buffer. Back()/this[Size-1] will now return this element.
        ///
        /// When the buffer is full, the element at Front()/this[0] will be popped to allow for this
        /// new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the back of the buffer</param>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                for (var i = 1; i < Capacity - 1; i++)
                    _buffer.Move(i, i + 1);

                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer.Add(item);
                ++Size;
            }
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreassing the Buffer size by 1.
        /// </summary>
        public void PopBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");

            Decrement(ref _end);
            _buffer[_end] = default;
            --Size;

            OnPropertyChanged(nameof(IsFull));
            OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// Removes the element at the front of the buffer. Decreassing the Buffer size by 1.
        /// </summary>
        public void PopFront()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");

            _buffer[_start] = default;
            Increment(ref _start);
            --Size;

            OnPropertyChanged(nameof(IsFull));
            OnPropertyChanged(nameof(IsEmpty));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
        {
            if (IsEmpty)
                throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Increment(ref int index)
        {
            if (++index == Capacity)
                index = 0;
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Decrement(ref int index)
        {
            if (index == 0)
                index = Capacity;

            index--;
        }

        /// <summary>
        /// Converts the index in the argument to an index in _buffer
        /// </summary>
        /// <returns>The transformed index.</returns>
        /// <param name="index">External index.</param>
        private int InternalIndex(int index)
        {
            return _start + (index < (Capacity - _start) ? index : index - Capacity);
        }

        // doing ArrayOne and ArrayTwo methods returning ArraySegment<T> as seen here:
        // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1957cccdcb0c4ef7d80a34a990065818d
        // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1f5081a54afbc2dfc1a7fb20329df7d5b
        // should help a lot with the code.

        // The array is composed by at most two non-contiguous segments, the next two methods allow
        // easy access to those.

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
