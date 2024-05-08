using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ObservableDictionary()
            : base()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> collection)
            : base(collection)
        {
            _dictionary = new Dictionary<TKey, TValue>(collection);
        }

        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
            : base()
        {
            _dictionary = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public ObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : base()
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, equalityComparer);
            _dictionary = new Dictionary<TKey, TValue>(capacity, equalityComparer);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> collection, IEqualityComparer<TKey> equalityComparer)
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
                _dictionary[key] = value;
                for (var i = 0; i < Count; i++)
                {
                    var entry = base[i];
                    if (entry.Key?.Equals(key) == true)
                    {
                        base[i] = new KeyValuePair<TKey, TValue>(key, value);
                        break;
                    }
                }
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Keys" />
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Values" />
        public ICollection<TValue> Values => _dictionary.Values;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Values" />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Values" />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Add(TKey, TValue)" />
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.ContainsKey(TKey)" />
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.Remove(TKey)" />
        public bool Remove(TKey key)
        {
            for (var i = 0; i < Count; i++)
            {
                var entry = base[i];
                if (entry.Key?.Equals(key) == true)
                {
                    base.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="System.Collections.Generic.Dictionary.TryGetValue(TKey, out TValue)" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
