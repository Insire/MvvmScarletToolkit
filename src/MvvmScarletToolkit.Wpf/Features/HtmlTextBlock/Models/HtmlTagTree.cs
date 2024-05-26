using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// html syntax tree
    /// </summary>
    internal sealed class HtmlTagTree : HtmlTagNode
    {
        private readonly IParamParser _paramParser;
        private readonly List<HTMLTagInfo> _builtinTags;

        public HtmlTagTree(IParamParser paramParser, List<HTMLTagInfo> builtinTags)
            : base(true, new HtmlTag(paramParser, "root", string.Empty, builtinTags))
        {
            _paramParser = paramParser ?? throw new ArgumentNullException(nameof(paramParser));
            _builtinTags = builtinTags ?? throw new ArgumentNullException(nameof(builtinTags));
        }

        /// <summary>
        ///  build syntax tree from input
        /// </summary>
        public void BuildFrom(string htmlInput)
        {
            HtmlTagNode? previousNode = this;

            var beforeTag = string.Empty;
            var afterTag = string.Empty;
            var name = string.Empty;
            var vars = string.Empty;

            do
            {
                ReadNextTag(htmlInput, ref beforeTag, ref afterTag, ref name, ref vars);

                if (beforeTag != string.Empty)
                {
                    AddTag(new HtmlTag(beforeTag, _builtinTags));
                }

                if (name != string.Empty)
                {
                    AddTag(new HtmlTag(_paramParser, name, vars, _builtinTags));
                }

                htmlInput = afterTag;
            }
            while (afterTag.Length > 0);

            void AddTag(HtmlTag tag)
            {
                while (previousNode?.CanAdd(tag) == false)
                {
                    previousNode = previousNode?.Parent;
                }

                previousNode = previousNode?.Add(tag);
            }
        }

        public override bool CanAdd(HtmlTag tag)
        {
            return true;
        }

        /// <summary>
        /// Parse a string and return text before a tag, the tag and it's variables, and the string after that tag.
        /// </summary>
        private static void ReadNextTag(string input, ref string beforeTag, ref string afterTag, ref string name, ref string vars)
        {
            var startIndex = input.IndexOf('<');
            var endIndex = input.IndexOf('>');

            if ((startIndex == -1) || (endIndex == -1) || (endIndex < startIndex))
            {
                vars = string.Empty;
                beforeTag = input;
                afterTag = string.Empty;
            }
            else
            {
                var tag = input.Substring(startIndex + 1, endIndex - startIndex - 1);
                beforeTag = input.Substring(0, startIndex);
                afterTag = input.Substring(endIndex + 1, input.Length - endIndex - 1);

                var pos3 = tag.IndexOf(' ');
                if ((pos3 != -1) && (tag != string.Empty))
                {
                    name = tag.Substring(0, pos3);
                    vars = tag.Substring(pos3 + 1, tag.Length - pos3 - 1);
                }
                else
                {
                    name = tag;
                    vars = string.Empty;
                }

                if (!name.StartsWith("!--"))
                {
                    return;
                }

                if ((name.Length < 6) || (!name.EndsWith("--")))
                {
                    var pos4 = afterTag.IndexOf("-->");
                    if (pos4 != -1)
                    {
                        afterTag = afterTag.Substring(pos4 + 2, afterTag.Length - pos4 - 1);
                    }
                }

                name = string.Empty;
                vars = string.Empty;
            }
        }
    }
}
