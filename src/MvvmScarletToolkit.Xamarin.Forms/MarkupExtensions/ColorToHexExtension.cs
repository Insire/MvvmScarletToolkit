using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MvvmScarletToolkit
{
    [Preserve(AllMembers = true)]
    [ContentProperty(nameof(Color))]
    [AcceptEmptyServiceProvider]
    public sealed class ColorToHexExtension : IMarkupExtension
    {
        public Color Color { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Color.ToHex();
        }
    }
}
