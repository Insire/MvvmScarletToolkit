using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a string is null or empty
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class IsNullOrEmpty : ConverterMarkupExtension<IsNullOrEmpty>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return string.IsNullOrEmpty(text);
            }

            return false;
        }
    }
}
