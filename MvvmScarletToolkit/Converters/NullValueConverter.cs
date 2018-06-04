using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    public sealed class NullValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            return string.IsNullOrEmpty(value as string) ? DependencyProperty.UnsetValue : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // According to https://msdn.microsoft.com/en-us/library/system.windows.data.ivalueconverter.convertback(v=vs.110).aspx#Anchor_1
            // (kudos Scott Chamberlain), if you do not support a conversion
            // back you should return a Binding.DoNothing or a
            // DependencyProperty.UnsetValue
            return Binding.DoNothing;
        }
    }
}
