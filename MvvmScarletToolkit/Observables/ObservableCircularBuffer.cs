using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly T[] _buffer;

        /// <summary>
        /// The _start. Index of the first element in buffer.
        /// </summary>
        private int _start;

        /// <summary>
        /// The _end. Index after the last element in the buffer.
        /// </summary>
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

            _buffer = new T[capacity];

            Array.Copy(items, _buffer, items.Length);
            Size = items.Length;

            _start = 0;
            _end = Size == capacity ? 0 : Size;
        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after maximum capacity is
        /// reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity => _buffer.Length;

        public bool IsFull => Size == Capacity;

        public bool IsEmpty => Size == 0;

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size { get; private set; }

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
        public void PushBack(T item)
        {
            var args = default(NotifyCollectionChangedEventArgs);
            var previousEntry = IsEmpty ? default : Back();

            if (_buffer[_end] == null)
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _end);

            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                ++Size;

                OnPropertyChanged(nameof(IsFull));
                OnPropertyChanged(nameof(IsEmpty));
            }

            if (args != null)
                OnCollectionChanged(args);
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object[] { item }, new object[] { previousEntry }, _end));
        }

        /// <summary>
        /// Pushes a new element to the front of the buffer. Front()/this[0] will now return this element.
        ///
        /// When the buffer is full, the element at Back()/this[Size-1] will be popped to allow for
        /// this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the front of the buffer</param>
        public void PushFront(T item)
        {
            var args = default(NotifyCollectionChangedEventArgs);
            var previousEntry = IsEmpty ? default : Front();

            if (_buffer[_end] == null)
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _end);

            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                ++Size;

                OnPropertyChanged(nameof(IsFull));
                OnPropertyChanged(nameof(IsEmpty));
            }

            if (args != null)
                OnCollectionChanged(args);
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object[] { item }, new object[] { previousEntry }, _end));
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreassing the Buffer size by 1.
        /// </summary>
        public void PopBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");

            var removeArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _buffer[_end], _end);

            Decrement(ref _end);
            _buffer[_end] = default;
            --Size;

            OnCollectionChanged(removeArgs);

            OnPropertyChanged(nameof(IsFull));
            OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// Removes the element at the front of the buffer. Decreassing the Buffer size by 1.
        /// </summary>
        public void PopFront()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");

            var removeArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _buffer[_start], _start);

            _buffer[_start] = default;
            Increment(ref _start);
            --Size;

            OnCollectionChanged(removeArgs);

            OnPropertyChanged(nameof(IsFull));
            OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// Copies the buffer contents to an array, acording to the logical contents of the buffer
        /// (i.e. independent of the internal order/contents)
        /// </summary>
        /// <returns>A new array with a copy of the buffer contents.</returns>
        public T[] ToArray()
        {
            var newArray = new T[Size];
            var newArrayOffset = 0;
            var segments = new ArraySegment<T>[2] { ArrayOne(), ArrayTwo() };

            foreach (ArraySegment<T> segment in segments)
            {
                Array.Copy(segment.Array, segment.Offset, newArray, newArrayOffset, segment.Count);
                newArrayOffset += segment.Count;
            }

            return newArray;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var segments = new ArraySegment<T>[2] { ArrayOne(), ArrayTwo() };

            foreach (ArraySegment<T> segment in segments)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    yield return segment.Array[segment.Offset + i];
                }
            }
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

        private ArraySegment<T> ArrayOne()
        {
            if (_start < _end)
                return new ArraySegment<T>(_buffer, _start, _end - _start);
            else
                return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }

        private ArraySegment<T> ArrayTwo()
        {
            if (_start < _end)
                return new ArraySegment<T>(_buffer, _end, 0);
            else
                return new ArraySegment<T>(_buffer, 0, _end);
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsFull)
            {
                for (var i = 0; i < this.Capacity; i++)
                {
                    if (this[i].Equals(e.NewItems))
                    {
                        Console.WriteLine(i);
                        break;
                    }
                }
            }

            CollectionChanged?.Invoke(this, e);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
