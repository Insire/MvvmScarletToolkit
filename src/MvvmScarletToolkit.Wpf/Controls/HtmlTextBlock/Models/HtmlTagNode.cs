using System;
using System.Collections;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Wpf
{
    internal class HtmlTagNode : IEnumerable
    {
        private readonly List<HtmlTagNode> _items;

        public bool IsRoot { get; }
        public HtmlTag Tag { get; }
        public HtmlTagNode Parent { get; }

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

        public virtual bool CanAdd(HtmlTag tag)
        {
            if (tag is null || Tag.IsEndTag)
            {
                return false;
            }

            return (tag.Name == '/' + Tag.Name) || (tag.Level < Tag.Level);
        }

        public HtmlTagNode Add(HtmlTag aTag)
        {
            if (!CanAdd(aTag))
            {
                throw new Exception();
            }

            var retVal = new HtmlTagNode(this, aTag);
            _items.Add(retVal);

            return aTag.Name == '/' + Tag.Name
                ? Parent
                : retVal;
        }
    }
}
