using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Convert a boolean value to corresponding inverted <see cref="System.Windows.Visibility"/> value
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class InvertBooleanToVisibilityConverter : ConverterMarkupExtension<InvertBooleanToVisibilityConverter>
    {
        [ConstructorArgument("visibility")]
        public Visibility Visibility { get; set; }

        public InvertBooleanToVisibilityConverter()
        {
            Visibility = Visibility.Hidden;
        }

        public InvertBooleanToVisibilityConverter(Visibility visibility)
        {
            Visibility = visibility;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool toggle)
            {
                if (toggle)
                {
                    return Visibility;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

            return Binding.DoNothing;
        }
    }
}
