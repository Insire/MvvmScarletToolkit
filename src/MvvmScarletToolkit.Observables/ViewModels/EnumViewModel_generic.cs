using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Observables
{
    public partial class EnumViewModel<T> : ViewModelContainer<T>
        where T : Enum
    {
        [ObservableProperty]
        private string _displayName;

        public EnumViewModel(in T value, in string? displayName)
            : base(value)
        {
            _displayName = displayName ?? "Undefined";
        }
    }
}
