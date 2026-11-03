namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// html syntax tree
    /// </summary>
    internal sealed class HtmlTagTree : HtmlTagNode
    {
        private readonly IParamParser _paramParser;

        public HtmlTagTree(IParamParser paramParser)
            : base(true, new HtmlTag(paramParser, "root", string.Empty))
        {
            _paramParser = paramParser ?? throw new ArgumentNullException(nameof(paramParser));
        }

        /// <summary>
        /// Builds the syntax tree from <paramref name="htmlInput"/>.
        /// </summary>
        /// <remarks>
        /// Scans the input by position, so only the text and tags that become part of the tree are copied.
        /// </remarks>
        public void BuildFrom(string htmlInput)
        {
            HtmlTagNode? previousNode = this;

            var input = htmlInput ?? string.Empty;
            var position = 0;

            while (position < input.Length)
            {
                var rest = input.AsSpan(position);
                var startIndex = rest.IndexOf('<');
                var endIndex = rest.IndexOf('>');

                // like before, a '>' ahead of the next '<' ends tag parsing, everything left is text
                if ((startIndex == -1) || (endIndex == -1) || (endIndex < startIndex))
                {
                    AddTag(new HtmlTag(input[position..]));
                    return;
                }

                if (startIndex > 0)
                {
                    AddTag(new HtmlTag(input.Substring(position, startIndex)));
                }

                var tag = rest.Slice(startIndex + 1, endIndex - startIndex - 1);
                position += endIndex + 1;

                if (tag.StartsWith("!--"))
                {
                    position = SkipComment(input, position, tag);
                    continue;
                }

                var separator = tag.IndexOf(' ');
                var name = separator == -1 ? tag : tag[..separator];
                if (name.IsEmpty)
                {
                    continue;
                }

                var vars = separator == -1 ? string.Empty : tag[(separator + 1)..].ToString();

                AddTag(new HtmlTag(_paramParser, name.ToString(), vars));
            }

            void AddTag(HtmlTag tag)
            {
                while (previousNode?.CanAdd(tag) == false)
                {
                    previousNode = previousNode?.Parent;
                }

                previousNode = previousNode?.Add(tag);
            }
        }

        public override bool CanAdd(HtmlTag? tag)
        {
            return true;
        }

        /// <summary>
        /// Returns the position after a comment that started with <paramref name="tag"/>.
        /// A comment ends at the first "--&gt;", which may come after a '&gt;' inside the comment.
        /// </summary>
        private static int SkipComment(string input, int position, ReadOnlySpan<char> tag)
        {
            // the comment already ended with the '>' that closed the tag, e.g. <!-- comment -->
            var name = tag.IndexOf(' ') is var separator and >= 0 ? tag[..separator] : tag;
            if (name.Length >= 6 && name.EndsWith("--"))
            {
                return position;
            }

            if (tag.EndsWith("--") && tag.Length >= 5)
            {
                return position;
            }

            var end = input.AsSpan(position).IndexOf("-->");

            return end == -1 ? position : position + end + 3;
        }
    }
}
