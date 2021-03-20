using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    ///<summary>
    /// whether a bound number is smaller than <see cref="Value"/>
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
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
            return value switch
            {
                sbyte number => number < Value,
                byte number => number < Value,
                short number => number < Value,
                ushort number => number < Value,
                int number => number < Value,
                uint number => number < Value,
                long number => number < Value,
                //case ulong number:
                //    return number < Value;
                float number => number < Value,
                double number => number < Value,
                decimal number => number < Value,
                _ => Binding.DoNothing,
            };
        }
    }
}
