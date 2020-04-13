using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Tests
{
    internal class DerivedObservableObject : ObservableObject
    {
        private readonly Action _onchanged;
        private readonly Action _onchanging;
        public object AutoProperty { get; set; }

        private object _notifyingProperty;
        public object NotifyingProperty
        {
            get { return _notifyingProperty; }
            set { SetValue(ref _notifyingProperty, value, OnChanged: _onchanged, OnChanging: _onchanging); }
        }

        public DerivedObservableObject()
        {
        }

        public DerivedObservableObject(Action onchanged, Action onchanging)
        {
            _onchanged = onchanged;
            _onchanging = onchanging;
        }
    }
}
