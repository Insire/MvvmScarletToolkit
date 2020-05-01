using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    public sealed class EnumViewModel
    {
        public static EnumViewModel<TEnum> Create<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return new EnumViewModel<TEnum>(value, value.GetAttributeOfType<DescriptionAttribute>()?.Description);
        }
    }
}
