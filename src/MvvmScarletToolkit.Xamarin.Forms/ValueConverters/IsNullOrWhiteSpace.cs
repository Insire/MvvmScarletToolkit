using System;
using System.Globalization;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a string is null or whitespace
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    public sealed class IsNullOrWhiteSpace : ConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return string.IsNullOrWhiteSpace(text);
            }

            return Binding.DoNothing;
        }
    }
}
