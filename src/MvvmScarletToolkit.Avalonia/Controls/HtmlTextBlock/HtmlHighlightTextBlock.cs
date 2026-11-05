using Avalonia;
using MvvmScarletToolkit.Html;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// An <see cref="HtmlTextBlock"/> that shows every occurrence of <see cref="Highlight"/> in bold, ignoring case. Only text is searched, not tags.
    /// </summary>
    /// <remarks>
    /// <code>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</code>
    /// </remarks>
    public class HtmlHighlightTextBlock : HtmlTextBlock
    {
        public static readonly StyledProperty<string?> HighlightProperty = AvaloniaProperty.Register<HtmlHighlightTextBlock, string?>(nameof(Highlight), string.Empty);

        public string? Highlight
        {
            get => GetValue(HighlightProperty);
            set => SetValue(HighlightProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HighlightProperty)
            {
                Render();
            }
        }

        protected override string PrepareHtml(string html)
        {
            return HtmlHighlighter.Apply(html, Highlight);
        }
    }
}
