using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a string is not null or whitespace
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class IsNotNullOrWhiteSpace : ConverterMarkupExtension<IsNotNullOrWhiteSpace>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return false;
            }

            if (value is string text)
            {
                return !string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}
