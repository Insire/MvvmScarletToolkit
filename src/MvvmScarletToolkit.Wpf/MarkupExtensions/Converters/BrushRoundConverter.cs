using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    [ValueConversion(typeof(SolidColorBrush), typeof(Brush))]
    public class BrushRoundConverter : ConverterMarkupExtension<BrushRoundConverter>
    {
        public Brush HighValue { get; set; } = Brushes.White;
        public Brush LowValue { get; set; } = Brushes.Black;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SolidColorBrush solidColorBrush))
                return Binding.DoNothing;

            var color = solidColorBrush.Color;

            var brightness = (0.3 * color.R) + (0.59 * color.G) + (0.11 * color.B);

            return brightness < 123 ? LowValue : HighValue;
        }
    }
}