using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(int), typeof(bool))]
    public sealed class GreaterThan : ConverterMarkupExtension<GreaterThan>
    {
        [ConstructorArgument("value")]
        public int Value { get; set; }

        public GreaterThan()
        {
            Value = 0;
        }

        public GreaterThan(int value)
        {
            Value = value;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return number > Value;
            }

            return false;
        }
    }
}
