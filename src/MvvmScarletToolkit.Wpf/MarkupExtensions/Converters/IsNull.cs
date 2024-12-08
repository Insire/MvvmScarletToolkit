using System;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether something is null
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(object))]
    public sealed class IsNull : ConverterMarkupExtension<IsNull>
    {
        public override object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is null;
        }
    }
}
