using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// whether a bound number is larger than <see cref="Value"/>
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(double?), typeof(bool))]
    public sealed class GreaterThan : MarkupExtension, IValueConverter
    {
        [ConstructorArgument("value")]
        public double? Value { get; set; }

        public GreaterThan()
        {
            Value = null;
        }

        public GreaterThan(int value)
        {
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var comparison = 0d;
            if (Value.HasValue)
            {
                comparison = Value.Value;
            }

            if (parameter is double @double)
            {
                comparison = @double;
            }

            if (parameter is string @string && double.TryParse(@string, out @double))
            {
                comparison = @double;
            }

            return value switch
            {
                sbyte number => number > comparison,
                byte number => number > comparison,
                short number => number > comparison,
                ushort number => number > comparison,
                int number => number > comparison,
                uint number => number > comparison,
                long number => number > comparison,
                ulong number => number > comparison,
                float number => number > comparison,
                double number => number > comparison,
                decimal number => System.Convert.ToDouble(number) > comparison,
                _ => Binding.DoNothing,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // According to https://msdn.microsoft.com/en-us/library/system.windows.data.ivalueconverter.convertback(v=vs.110).aspx#Anchor_1
            // (kudos Scott Chamberlain), if you do not support a conversion
            // back you should return a Binding.DoNothing or a DependencyProperty.UnSetProperty
            return Binding.DoNothing;
        }
    }
}
