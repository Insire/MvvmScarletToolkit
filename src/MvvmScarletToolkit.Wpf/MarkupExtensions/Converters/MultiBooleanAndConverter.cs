using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// combines all conditions with logical and
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
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
