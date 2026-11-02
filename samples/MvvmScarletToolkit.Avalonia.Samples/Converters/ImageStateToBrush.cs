using Avalonia.Media;
using MvvmScarletToolkit.Core.Samples.Features.Image;
using System.Globalization;

namespace MvvmScarletToolkit.Avalonia.Samples.Converters
{
    public sealed class ImageStateToBrush : ConverterMarkupExtension<ImageStateToBrush>
    {
        public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value switch
            {
                VirtualizedImageState.Pending => Brushes.Khaki,
                VirtualizedImageState.Loading => Brushes.Orange,
                VirtualizedImageState.Failed => Brushes.IndianRed,
                VirtualizedImageState.Shown => Brushes.MediumSeaGreen,
                VirtualizedImageState.Unrealized => Brushes.SteelBlue,
                _ => Brushes.LightGray,
            };
        }
    }
}
