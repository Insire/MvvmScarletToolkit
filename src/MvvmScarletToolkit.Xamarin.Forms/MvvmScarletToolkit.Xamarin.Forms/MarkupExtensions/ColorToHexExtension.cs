using System;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace MvvmScarletToolkit
{
    [Preserve(AllMembers = true)]
    [ContentProperty(nameof(Color))]
    public sealed class ColorToHexExtension : IMarkupExtension
    {
        public Color Color { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var hex = Color.ToHex();

            return hex;
        }
    }
}
