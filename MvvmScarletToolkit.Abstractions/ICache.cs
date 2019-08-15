using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ICache
    {
        bool Contains(string key);

        bool Add(string key, object data);

        event EventHandler<CachedValueRemovedEventArgs> ItemRemoved;
    }

    public sealed class CachedValueRemovedEventArgs : EventArgs
    {
        public object Value { get; set; }

        public CachedValueRemovedEventArgs(object value)
        {
            Value = value;
        }
    }
}
