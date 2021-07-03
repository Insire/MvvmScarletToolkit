using System;
using System.Globalization;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether something is not null
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    public sealed class IsNotNull : ConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is null);
        }
    }
}
