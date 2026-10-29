using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class
        /// that contains elements copied from the specified <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> whose elements are copied to the new <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
        /// <exception cref="System.ArgumentNullException">dictionary is null</exception>
        /// <exception cref="System.ArgumentException">dictionary contains one or more duplicate keys</exception>
        public ObservableDictionary(IDictionary<TKey, TValue> collection)
            : base(collection)
        {
            _dictionary = new Dictionary<TKey, TValue>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class
        /// and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="equalityComparer">The <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{TKey}"/> for the type of the key.</param>
        public ObservableDictionary(IEqualityComparer<TKey>? equalityComparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class
        /// that is empty, has the specified initial capacity,
        /// and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ObservableDictionary{TKey, TValue}"/> can contain..</param>
        /// <param name="equalityComparer">The <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/> implementation to use whencomparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{TKey}"/>for the type of the key.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0</exception>
        public ObservableDictionary(int capacity, IEqualityComparer<TKey>? equalityComparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class
        /// that contains elements copied from the specified <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>
        /// and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> whose elements are copied to the new <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
        /// <param name="equalityComparer">The <see cref="System.Collections.Generic.IEqualityComparer{TKey}"/> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{TKey}"/> for the type of the key.</param>
        /// <exception cref="System.ArgumentNullException">dictionary is null</exception>
        /// <exception cref="System.ArgumentException">dictionary contains one or more duplicate keys</exception>
        public ObservableDictionary(IDictionary<TKey, TValue> collection, IEqualityComparer<TKey>? equalityComparer)
            : base(collection)
        {
            _dictionary = new Dictionary<TKey, TValue>(collection, equalityComparer);
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.this[TKey]" />
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                var item = new KeyValuePair<TKey, TValue>(key, value);
                var index = _dictionary.ContainsKey(key)
                    ? IndexOfKey(key)
                    : -1;

                if (index == -1)
                {
                    base.Add(item);
                }
                else
                {
                    base[index] = item;
                }
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Keys" />
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Values" />
        public ICollection<TValue> Values => _dictionary.Values;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Values" />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Values" />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Add(TKey, TValue)" />
        public void Add(TKey key, TValue value)
        {
            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.ContainsKey(TKey)" />
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.Remove(TKey)" />
        public bool Remove(TKey key)
        {
            if (!_dictionary.ContainsKey(key))
            {
                return false;
            }

            var index = IndexOfKey(key);
            if (index == -1)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            _dictionary.Clear();
            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, KeyValuePair<TKey, TValue> item)
        {
            // throws for null and duplicate keys, so the list and the dictionary can't get out of sync
            _dictionary.Add(item.Key, item.Value);
            base.InsertItem(index, item);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var entry = base[index];

            if (_dictionary.Remove(entry.Key))
            {
                base.RemoveItem(index);
            }
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, KeyValuePair<TKey, TValue> item)
        {
            var oldKey = base[index].Key;
            var comparer = _dictionary.Comparer;

            if (!comparer.Equals(oldKey, item.Key))
            {
                if (_dictionary.ContainsKey(item.Key))
                {
                    throw new ArgumentException("An item with the same key has already been added.", nameof(item));
                }

                _dictionary.Remove(oldKey);
            }

            _dictionary[item.Key] = item.Value;
            base.SetItem(index, item);
        }

        private int IndexOfKey(TKey key)
        {
            var comparer = _dictionary.Comparer;
            for (var i = 0; i < Count; i++)
            {
                if (comparer.Equals(base[i].Key, key))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
