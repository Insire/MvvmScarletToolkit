using System;

namespace MvvmScarletToolkit.Abstractions
{
    public sealed class CachedValueRemovedEventArgs : EventArgs
    {
        public object Value { get; set; }

        public CachedValueRemovedEventArgs(object value)
        {
            Value = value;
        }
    }
}
