using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(string), typeof(object))]
    public sealed class NullValueConverter : ConverterMarkupExtension<NullValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null
                ? DependencyProperty.UnsetValue
                : string.IsNullOrEmpty(value as string)
                    ? DependencyProperty.UnsetValue
                    : value;
        }
    }
}
