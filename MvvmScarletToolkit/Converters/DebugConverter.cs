using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class DebugConverter : ConverterMarkupExtension<DebugConverter>
    {
        private readonly Action<string> Logger = (message) => Debug.WriteLine(message);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
            {
                Logger(string.Format(culture, "Convert: '{0}' to '{1}'", value, targetType));
            }
            else
            {
                Logger(string.Format(culture, "Convert: '{0}' to '{1}' with Parameter = '{2}' of type: = '{3}'", value, targetType, parameter, parameter.GetType().Name));
            }

            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
            {
                Logger(string.Format(culture, "ConvertBack: '{0}' to '{1}'", value, targetType));
            }
            else
            {
                Logger(string.Format(culture, "ConvertBack: '{0}' to '{1}' with Parameter = '{2}' of type: = '{3}'", value, targetType, parameter, parameter.GetType().Name));
            }

            return value;
        }
    }
}
