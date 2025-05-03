using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// alter the casing for a string
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class ToCase : ConverterMarkupExtension<ToCase>
    {
        public CharacterCasing Casing { get; set; }

        public ToCase()
        {
            Casing = CharacterCasing.Upper;
        }

        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                var casing = Casing;
                if (parameter is CharacterCasing argCasing)
                {
                    casing = argCasing;
                }

                return Convert(str, parameter, casing);
            }

            return value;
        }

        private static object Convert(string value, object? parameter, CharacterCasing fallBackCasing)
        {
            if (parameter is CharacterCasing characterCasing)
            {
                return LocalConvert(characterCasing);
            }
            else
            {
                return LocalConvert(fallBackCasing);
            }

            object LocalConvert(CharacterCasing casing)
            {
                return casing switch
                {
                    CharacterCasing.Lower => value.ToLowerInvariant(),
                    CharacterCasing.Normal => value,
                    CharacterCasing.Upper => value.ToUpperInvariant(),
                    _ => value,
                };
            }
        }
    }
}
