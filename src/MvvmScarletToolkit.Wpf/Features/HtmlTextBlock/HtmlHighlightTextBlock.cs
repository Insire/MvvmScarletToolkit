using System.Text.RegularExpressions;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public class HtmlHighlightTextBlock : HtmlTextBlock
    {
        private static readonly Regex _tagSplitter = new Regex("(<[^>]*>)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

            var highlight = Highlight;
            if (!string.IsNullOrEmpty(highlight))
            {
                var pattern = Regex.Escape(highlight);

                // only highlight text between tags, the split keeps the tags, since they are captured
                var segments = _tagSplitter.Split(html);
                for (var i = 0; i < segments.Length; i++)
                {
                    if (!segments[i].StartsWith('<'))
                    {
                        segments[i] = Regex.Replace(segments[i], pattern, "<b>$0</b>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }

                html = string.Concat(segments);
            }

            this.UpdateWith(html);
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
