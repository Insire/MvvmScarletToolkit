using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class InvertBooleanToVisibilityConverter : ConverterMarkupExtension<InvertBooleanToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool toggle)
            {
                if (toggle)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }

            return Binding.DoNothing;
        }
    }
}
