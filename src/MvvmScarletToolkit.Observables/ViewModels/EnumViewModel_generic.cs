using System;

namespace MvvmScarletToolkit.Observables
{
    public sealed class EnumViewModel<T> : ViewModelContainer<T>
        where T : Enum
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }

        public EnumViewModel(in T value, in string? displayName)
            : base(value)
        {
            _displayName = displayName ?? "Undefined";
        }
    }
}
