using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(string), typeof(string))]
    public class CaseConverter : ConverterMarkupExtension<CaseConverter>
    {
        public CharacterCasing Case { get; set; }

        public CaseConverter()
        {
            Case = CharacterCasing.Upper;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return Convert(str, parameter, Case);
            }

            return string.Empty;
        }

        private static object Convert(string value, object parameter, CharacterCasing fallBackCasing)
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
                    CharacterCasing.Lower => value.ToLower(),
                    CharacterCasing.Normal => value,
                    CharacterCasing.Upper => value.ToUpper(),
                    _ => value,
                };
            }
        }
    }
}
