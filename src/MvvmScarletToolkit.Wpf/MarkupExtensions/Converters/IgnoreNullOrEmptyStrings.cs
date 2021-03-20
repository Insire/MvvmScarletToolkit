using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    ///<summary>
    /// Filter/ignore strings that are null or empty
    ///</summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string), typeof(object))]
    public sealed class IgnoreNullOrEmptyStrings : ConverterMarkupExtension<IgnoreNullOrEmptyStrings>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return DependencyProperty.UnsetValue;

                default:
                    if (string.IsNullOrEmpty(value as string))
                    {
                        return DependencyProperty.UnsetValue;
                    }
                    else
                    {
                        return value;
                    }
            }
        }
    }
}
