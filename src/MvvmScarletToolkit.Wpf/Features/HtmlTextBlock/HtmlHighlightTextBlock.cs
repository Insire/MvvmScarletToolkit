using MvvmScarletToolkit.Html;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public class HtmlHighlightTextBlock : HtmlTextBlock
    {
        /// <summary>Identifies the <see cref="Highlight"/> dependency property.</summary>
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register(
            nameof(Highlight),
            typeof(string),
            typeof(HtmlHighlightTextBlock),
            new UIPropertyMetadata(string.Empty, OnHighlightChanged));

        public string Highlight
        {
            get => (string)GetValue(HighlightProperty);
            set => SetValue(HighlightProperty, value);
        }

        protected override void Parse(string html)
        {
            Inlines.Clear();

            if (string.IsNullOrEmpty(html))
            {
                return;
            }

            this.UpdateWith(HtmlHighlighter.Apply(html, Highlight));
        }

        private static void OnHighlightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlHighlightTextBlock textBlock)
            {
                textBlock.Parse(textBlock.Html);
            }
        }
    }
}
