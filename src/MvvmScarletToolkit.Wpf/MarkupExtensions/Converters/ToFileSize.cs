using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Converts a size in bytes to a readable size.
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(long), typeof(string))]
    public sealed class ToFileSize : ConverterMarkupExtension<ToFileSize>
    {
        private static readonly string[] _units = ["B", "KB", "MB", "GB", "TB", "PB"];

        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not long bytes)
            {
                return string.Empty;
            }

            double size = bytes;
            var unit = 0;
            while (size >= 1024 && unit < _units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            var format = unit == 0 ? "N0" : size < 10 ? "N2" : "N1";

            return $"{size.ToString(format, CultureInfo.CurrentCulture)} {_units[unit]}";
        }
    }
}
