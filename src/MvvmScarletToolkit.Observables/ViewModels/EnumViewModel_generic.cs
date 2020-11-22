using System;

namespace MvvmScarletToolkit.Observables
{
    public class EnumViewModel<T> : ViewModelContainer<T>
        where T : Enum
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        public EnumViewModel(in T value, in string? displayName)
            : base(value)
        {
            _displayName = displayName ?? "Undefined";
        }
    }
}
