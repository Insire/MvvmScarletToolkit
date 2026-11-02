using System.Globalization;

namespace MvvmScarletToolkit.Avalonia.Samples.Converters
{
    /// <summary>
    /// Takes "Upper", "Lower" or nothing as parameter
    /// </summary>
    public sealed class ToCase : ConverterMarkupExtension<ToCase>
    {
        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            var text = value as string;

            return (parameter as string) switch
            {
                "Upper" => text?.ToUpper(culture ?? CultureInfo.CurrentCulture),
                "Lower" => text?.ToLower(culture ?? CultureInfo.CurrentCulture),
                _ => text,
            };
        }
    }
}
