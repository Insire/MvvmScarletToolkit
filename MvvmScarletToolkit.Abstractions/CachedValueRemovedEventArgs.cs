using System;

namespace MvvmScarletToolkit.Abstractions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "Class is not meant to be bound.")]
    public sealed class CachedValueRemovedEventArgs : EventArgs
    {
        public object Value { get; set; }

        public CachedValueRemovedEventArgs(object value)
        {
            Value = value;
        }
    }
}
