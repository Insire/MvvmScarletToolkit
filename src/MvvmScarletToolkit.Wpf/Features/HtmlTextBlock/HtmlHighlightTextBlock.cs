using System.Net;
using System.Text;
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
                var pattern = new Regex(Regex.Escape(highlight), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                // only highlight text between tags, the split keeps the tags, since they are captured
                var segments = _tagSplitter.Split(html);
                for (var i = 0; i < segments.Length; i++)
                {
                    if (!segments[i].StartsWith('<'))
                    {
                        segments[i] = ApplyHighlight(segments[i], pattern);
                    }
                }

                html = string.Concat(segments);
            }

            this.UpdateWith(html);
        }

        /// <summary>
        /// Matches against the decoded text, so "R&amp;D" is found in "R&amp;amp;D" and a match never splits an entity like "&amp;amp;".
        /// </summary>
        private static string ApplyHighlight(string segment, Regex pattern)
        {
            var text = WebUtility.HtmlDecode(segment);
            var matches = pattern.Matches(text);
            if (matches.Count == 0)
            {
                return segment;
            }

            var builder = new StringBuilder(segment.Length + (matches.Count * 7));
            var last = 0;
            foreach (Match match in matches)
            {
                builder.Append(WebUtility.HtmlEncode(text[last..match.Index]));
                builder.Append("<b>").Append(WebUtility.HtmlEncode(match.Value)).Append("</b>");
                last = match.Index + match.Length;
            }

            builder.Append(WebUtility.HtmlEncode(text[last..]));

            return builder.ToString();
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
