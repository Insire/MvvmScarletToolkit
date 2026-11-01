using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Returns the width passed as <see cref="Binding.ConverterParameter"/> when the value is <see langword="true"/>, otherwise 0.
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class BooleanToWidth : ConverterMarkupExtension<BooleanToWidth>
    {
        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is true
                ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture)
                : 0d;
        }
    }
}
