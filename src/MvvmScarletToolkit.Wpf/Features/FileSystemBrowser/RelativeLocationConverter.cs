using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Converts the location of an entry and the path of the searched folder to the path between them.
    /// Results from the searched folder itself show its name.
    /// </summary>
    public sealed class RelativeLocationConverter : IMultiValueConverter
    {
        public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is not [string location, string scope])
            {
                return string.Empty;
            }

            var relative = Path.GetRelativePath(scope, location);

            return relative == "." ? Path.GetFileName(location.TrimEnd(Path.DirectorySeparatorChar)) : relative;
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            return [DependencyProperty.UnsetValue, DependencyProperty.UnsetValue];
        }
    }
}
