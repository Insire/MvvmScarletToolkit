using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MvvmScarletToolkit
{
    [Preserve(AllMembers = true)]
    [ContentProperty(nameof(Color))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "One does not bind to markupextensions, they are the target for value setting, not the source of value generation")]
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
