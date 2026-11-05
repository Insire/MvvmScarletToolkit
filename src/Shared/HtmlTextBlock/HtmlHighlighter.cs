using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MvvmScarletToolkit.Html
{
    /// <summary>
    /// Wraps every occurrence of a text in <c>b</c> tags, ignoring case. Only text is searched, not tags.
    /// </summary>
    internal static class HtmlHighlighter
    {
        private static readonly Regex _tagSplitter = new Regex("(<[^>]*>)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Apply(string html, string? highlight)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(highlight))
            {
                return html;
            }

            var pattern = new Regex(Regex.Escape(highlight), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // only highlight text between tags, the split keeps the tags, since they are captured
            var segments = _tagSplitter.Split(html);
            for (var i = 0; i < segments.Length; i++)
            {
                if (!segments[i].StartsWith('<'))
                {
                    segments[i] = ApplyToText(segments[i], pattern);
                }
            }

            return string.Concat(segments);
        }

        /// <summary>
        /// Matches against the decoded text, so "R&amp;D" is found in "R&amp;amp;D" and a match never splits an entity like "&amp;amp;".
        /// </summary>
        private static string ApplyToText(string segment, Regex pattern)
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
    }
}
