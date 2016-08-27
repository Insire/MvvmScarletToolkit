using System.Collections.Generic;
using System.Collections.Specialized;

namespace MvvmScarletToolkit
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : ObservableObject, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        new public virtual void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        new public virtual void Clear()
        {
            base.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        new public virtual void Remove(TKey key)
        {
            base.Remove(key);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }
    }
}
