using System;
using System.ComponentModel;

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

        public EnumViewModel(T value, string? displayName)
            : base(value)
        {
            _displayName = displayName ?? "Undefined";
        }
    }

    public sealed class EnumViewModel
    {
        public static EnumViewModel<TEnum> Create<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return new EnumViewModel<TEnum>(value, value.GetAttributeOfType<DescriptionAttribute>()?.Description);
        }
    }
}
