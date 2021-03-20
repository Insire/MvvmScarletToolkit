using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(bool), typeof(object))]
    public sealed class IsNull : ConverterMarkupExtension<IsNullOrEmpty>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null;
        }
    }
}
