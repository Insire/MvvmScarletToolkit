using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> to a local time string in the current culture.
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(DateTime), typeof(string))]
    public sealed class ToLocalTime : ConverterMarkupExtension<ToLocalTime>
    {
        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is DateTime time
                ? time.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
                : string.Empty;
        }
    }
}
