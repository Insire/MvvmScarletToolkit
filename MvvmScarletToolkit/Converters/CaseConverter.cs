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
                switch (Case)
                {
                    case CharacterCasing.Lower:
                        return str.ToLower();

                    case CharacterCasing.Normal:
                        return str;

                    case CharacterCasing.Upper:
                        return str.ToUpper();

                    default:
                        return str;
                }
            }
            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
