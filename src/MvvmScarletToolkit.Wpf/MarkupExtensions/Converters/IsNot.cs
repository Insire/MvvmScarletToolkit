using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// negate a boolean value
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class IsNot : ConverterMarkupExtension<IsNot>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }

            return false;
        }
    }
}
