using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    ///<summary>
    /// Compare the <see cref="Binding.Path"/> value with the <see cref="Binding.ConverterParameter"/> value and return whether they are <see cref="object.Equals(object, object)"/>
    ///</summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class RadioButtonCheckedConverter : ConverterMarkupExtension<RadioButtonCheckedConverter>
    {
        /// <summary>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        /// <summary>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </summary>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
