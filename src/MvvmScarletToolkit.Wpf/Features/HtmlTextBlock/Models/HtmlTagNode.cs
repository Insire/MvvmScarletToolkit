using System.Collections;

namespace MvvmScarletToolkit.Wpf
{
    internal class HtmlTagNode : IEnumerable
    {
        private readonly List<HtmlTagNode> _items;

        public bool IsRoot { get; }
        public HtmlTag Tag { get; }
        public HtmlTagNode? Parent { get; }

        protected HtmlTagNode(bool isRoot, HtmlTag tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
            IsRoot = isRoot;

            _items = new List<HtmlTagNode>();
        }

        public HtmlTagNode(HtmlTagNode parent, HtmlTag tag)
            : this(false, tag)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public virtual bool CanAdd(HtmlTag? tag)
        {
            if (tag is null || Tag.IsEndTag)
            {
                return false;
            }

            return (tag.Name == Tag.EndTagName) || (tag.Level < Tag.Level);
        }

        public HtmlTagNode? Add(HtmlTag tag)
        {
            if (!CanAdd(tag))
            {
                throw new Exception();
            }

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
}
