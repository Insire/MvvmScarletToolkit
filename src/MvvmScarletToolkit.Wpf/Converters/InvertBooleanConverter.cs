using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class InvertBooleanConverter : ConverterMarkupExtension<InvertBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool toggle)
                return !toggle;

            return Binding.DoNothing;
        }
    }
}
