using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class DebugConverter : ConverterMarkupExtension<DebugConverter>
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();

        public Action<string, string> Logger { get; set; } = (message, category) => Debug.WriteLine(message, category);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                Logger(string.Format(culture, "Convert: Value = '{0}' TargetType = '{1}'", value, targetType), Name);
            else
                Logger(string.Format(culture, "Convert: Value = '{0}' TargetType = '{1}' Parameter = '{2}'", value, targetType, parameter), Name);

            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                Logger(string.Format(culture, "ConvertBack: Value = '{0}' TargetType = '{1}'", value, targetType), Name);
            else
                Logger(string.Format(culture, "ConvertBack: Value = '{0}' TargetType = '{1}' Parameter = '{2}'", value, targetType, parameter), Name);

            return value;
        }
    }
}
