using System;
using System.Globalization;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// negate a boolean value
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    public sealed class IsNot : ConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }

            return Binding.DoNothing;
        }
    }
}
