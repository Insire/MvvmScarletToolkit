using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a string is null or whitespace
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class IsNullOrWhiteSpace : ConverterMarkupExtension<IsNullOrWhiteSpace>
    {
        public override object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is null)
            {
                return true;
            }

            if (value is string text)
            {
                return string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}
