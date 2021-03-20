using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class IsNotNullOrWhiteSpace : ConverterMarkupExtension<IsNullOrEmpty>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return !string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}
