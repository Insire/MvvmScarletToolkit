using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(int), typeof(bool))]
    public sealed class SmallerThan : ConverterMarkupExtension<SmallerThan>
    {
        [ConstructorArgument("value")]
        public int Value { get; set; }

        public SmallerThan()
        {
            Value = 0;
        }

        public SmallerThan(int value)
        {
            Value = value;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return number < Value;
            }

            return false;
        }
    }
}
