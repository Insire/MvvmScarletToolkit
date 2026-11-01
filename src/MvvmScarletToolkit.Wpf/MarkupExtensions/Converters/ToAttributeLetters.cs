using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Converts <see cref="FileAttributes"/> to the letters known from the windows explorer, e.g. RHSA.
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(FileAttributes), typeof(string))]
    public sealed class ToAttributeLetters : ConverterMarkupExtension<ToAttributeLetters>
    {
        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not FileAttributes attributes)
            {
                return string.Empty;
            }

            var letters = new StringBuilder(4);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                letters.Append('R');
            }

            if (attributes.HasFlag(FileAttributes.Hidden))
            {
                letters.Append('H');
            }

            if (attributes.HasFlag(FileAttributes.System))
            {
                letters.Append('S');
            }

            if (attributes.HasFlag(FileAttributes.Archive))
            {
                letters.Append('A');
            }

            return letters.ToString();
        }
    }
}
