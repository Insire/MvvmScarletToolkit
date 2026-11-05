namespace MvvmScarletToolkit.Html
{
    internal class HtmlTagNode
    {
        private readonly List<HtmlTagNode> _items = new List<HtmlTagNode>();

        public HtmlTag Tag { get; }

        public HtmlTagNode? Parent { get; }

        protected HtmlTagNode(HtmlTag tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        private HtmlTagNode(HtmlTagNode parent, HtmlTag tag)
            : this(tag)
        {
            Parent = parent;
        }

        public virtual bool CanAdd(HtmlTag tag)
        {
            if (Tag.IsEndTag)
            {
                return false;
            }

            return (tag.Name == Tag.EndTagName) || (tag.Level < Tag.Level);
        }

        public HtmlTagNode? Add(HtmlTag tag)
        {
            var result = new HtmlTagNode(this, tag);
            _items.Add(result);

            return tag.Name == Tag.EndTagName
                ? Parent
                : result;
        }

        /// <summary>
        /// Returns the known tags of this node and its descendants in document order.
        /// </summary>
        public IEnumerable<HtmlTag> GetTags()
        {
            // iterative, nested iterators would cost the depth of the tree for every tag
            var stack = new Stack<HtmlTagNode>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node.Tag.Id != -1)
                {
                    yield return node.Tag;
                }

                for (var i = node._items.Count - 1; i >= 0; i--)
                {
                    stack.Push(node._items[i]);
                }
            }
        }
    }

    /// <summary>
    /// html syntax tree
    /// </summary>
    internal sealed class HtmlTagTree : HtmlTagNode
    {
        public HtmlTagTree()
            : base(new HtmlTag("root", string.Empty))
        {
        }

        public override bool CanAdd(HtmlTag tag)
        {
            return true;
        }

        /// <summary>
        /// Builds the syntax tree from <paramref name="htmlInput"/>.
        /// </summary>
        /// <remarks>
        /// Scans the input by position, so only the text and tags that become part of the tree are copied.
        /// </remarks>
        public void BuildFrom(string? htmlInput)
        {
            HtmlTagNode? previousNode = this;

            var input = htmlInput ?? string.Empty;
            var position = 0;

            while (position < input.Length)
            {
                var rest = input.AsSpan(position);
                var startIndex = rest.IndexOf('<');
                var endIndex = rest.IndexOf('>');

                // a '>' ahead of the next '<' ends tag parsing, everything left is text
                if ((startIndex == -1) || (endIndex == -1) || (endIndex < startIndex))
                {
                    AddTag(HtmlTag.CreateText(input[position..]));
                    return;
                }

                if (startIndex > 0)
                {
                    AddTag(HtmlTag.CreateText(input.Substring(position, startIndex)));
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

                var attributes = separator == -1 ? string.Empty : tag[(separator + 1)..].ToString();

                AddTag(new HtmlTag(name.ToString(), attributes));
            }

            void AddTag(HtmlTag tag)
            {
                while (previousNode?.CanAdd(tag) == false)
                {
                    previousNode = previousNode.Parent;
                }

                previousNode = previousNode?.Add(tag);
            }
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
