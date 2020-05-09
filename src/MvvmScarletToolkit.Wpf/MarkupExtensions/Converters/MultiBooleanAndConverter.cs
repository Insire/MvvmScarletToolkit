using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class MultiBooleanAndConverter : MultiConverterMarkupExtension<MultiBooleanAndConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = true;
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] is bool newValue)
                {
                    result &= newValue;
                }
                else
                {
                    return false;
                }
            }

            return result;
        }
    }
}
