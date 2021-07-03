using System;
using System.Globalization;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a string is not null or empty
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    public sealed class IsNotNullOrEmpty : ConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return !string.IsNullOrEmpty(text);
            }

            return Binding.DoNothing;
        }
    }
}
