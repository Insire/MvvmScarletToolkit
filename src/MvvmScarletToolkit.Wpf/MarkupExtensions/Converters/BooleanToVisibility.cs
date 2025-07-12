using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Convert a boolean value to corresponding <see cref="System.Windows.Visibility"/> value
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibility : ConverterMarkupExtension<InvertBooleanToVisibility>
    {
        public override object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool toggle && toggle)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }
    }
}
