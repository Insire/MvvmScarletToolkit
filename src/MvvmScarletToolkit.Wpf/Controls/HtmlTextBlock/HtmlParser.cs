using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;

namespace MvvmScarletToolkit.Wpf
{
    internal static class HtmlParser
    {
        public static HTMLTagInfo[] BuiltinTags { get; } = new HTMLTagInfo[51]
        {
           //HtmlTag Level guide
           // 50 Master
           // 40 Xml
           // 30 var(Variables tag for search)
           // 15 Html
           // 14 Title, Head, Body
           // 13 selection, hi, DIV, SPAN
           // 12 Table, centre, Form
           // 11 Tr (Table Row)
           // 10 Td, Th (Table Cell, Header)
           // 09 ul, ol (Numbering)
           // 08 li (List), Indent, blockquote
           // 07 P (Paragraph),  H1-H6
           // 06
           // 05 A HtmlTag, Input
           // 04 Text formating tags (B, strong, U, S, I, FONT, sub, sup),
           // 03
           // 02
           // 01 Unknown Tags, script
           // 00 Text, hr, user, Img, Dynamic, BR, Meta, Binding
           new HTMLTagInfo ("master",       HTMLFlag.Region,        50),
           new HTMLTagInfo ("xml",          HTMLFlag.Xml,           40),
           new HTMLTagInfo ("var",          HTMLFlag.Search,        30),
           new HTMLTagInfo ("html",         HTMLFlag.Region,        15),
           new HTMLTagInfo ("head",         HTMLFlag.Region,        14),
           new HTMLTagInfo ("body",         HTMLFlag.Region,        14),
           new HTMLTagInfo ("title",        HTMLFlag.Region,        14),
           new HTMLTagInfo ("div",          HTMLFlag.Region,        13),
           new HTMLTagInfo ("selection",    HTMLFlag.TextFormat,    13),
           new HTMLTagInfo ("hi",           HTMLFlag.TextFormat,    13),
           new HTMLTagInfo ("table",        HTMLFlag.Table,         13),
           new HTMLTagInfo ("centre",       HTMLFlag.Region,        13),
           new HTMLTagInfo ("form",         HTMLFlag.Controls,      12),
           new HTMLTagInfo ("tr",           HTMLFlag.Table,         11),
           new HTMLTagInfo ("td",           HTMLFlag.Table,         10),
           new HTMLTagInfo ("th",           HTMLFlag.Table,         10),
           new HTMLTagInfo ("ul",           HTMLFlag.Region,        09),
           new HTMLTagInfo ("ol",           HTMLFlag.Region,        09),
           new HTMLTagInfo ("li",           HTMLFlag.Region,        08),
           new HTMLTagInfo ("blockquote",   HTMLFlag.TextFormat,    08),
           new HTMLTagInfo ("indent",       HTMLFlag.Region,        08),
           new HTMLTagInfo ("p",            HTMLFlag.Region,        07),
           new HTMLTagInfo ("h1",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("h2",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("h3",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("h4",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("h5",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("h6",           HTMLFlag.Region,        07),
           new HTMLTagInfo ("span",         HTMLFlag.Region,        07),
           new HTMLTagInfo ("font",         HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("u",            HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("b",            HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("s",            HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("i",            HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("a",            HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("sup",          HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("sub",          HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("strong",       HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("color",        HTMLFlag.TextFormat,    04),
           new HTMLTagInfo ("input",        HTMLFlag.Controls,      02),
           new HTMLTagInfo ("select",       HTMLFlag.Controls,      02),
           new HTMLTagInfo ("option",       HTMLFlag.Controls,      02),
           new HTMLTagInfo ("script",       HTMLFlag.None,          01),
           new HTMLTagInfo ("meta",         HTMLFlag.Variable,      00),
           new HTMLTagInfo ("br",           HTMLFlag.Element,       00),
           new HTMLTagInfo ("hr",           HTMLFlag.Element,       00),
           new HTMLTagInfo ("img",          HTMLFlag.Element,       00),
           new HTMLTagInfo ("text",         HTMLFlag.Element,       00),
           new HTMLTagInfo ("binding",      HTMLFlag.Element,       00),
           new HTMLTagInfo ("dynamic",      HTMLFlag.Dynamic,       00),
           new HTMLTagInfo ("user",         HTMLFlag.Dynamic,       00),
       };

        public static IParamParser ParamParser { get; set; } = new ParamParser(new HtmlAttributeStringSerializer());

        public static void UpdateWith(this TextBlock textBlock, string htmlInput)
        {
            textBlock.UpdateWith(htmlInput, ParamParser ?? new ParamParser(new HtmlAttributeStringSerializer()));
        }

        public static void UpdateWith(this TextBlock textBlock, string htmlInput, IParamParser paramParser)
        {
            var tree = new HtmlTagTree(paramParser);
            HtmlTagNode previousNode = tree;

            var beforeTag = string.Empty;
            var afterTag = string.Empty;
            var tagName = string.Empty;
            var tagVar = string.Empty;

            // build syntax tree
            do
            {
                ReadNextTag(htmlInput, ref beforeTag, ref afterTag, ref tagName, ref tagVar);

                if (beforeTag != string.Empty)
                {
                    AddTag(new HtmlTag(beforeTag));
                }

                if (tagName != string.Empty)
                {
                    AddTag(new HtmlTag(paramParser, tagName, tagVar));
                }

                htmlInput = afterTag;
            }
            while (afterTag.Length > 0);

            // update textbox with inline elements according to syntax tree items
            var currentStateType = new CurrentStateType();
            var tags = tree.GetTags();

            foreach (var tag in tags.Where(tag => tag.ID != -1).Select(tag => tag))
            {
                switch (BuiltinTags[tag.ID].Flags)
                {
                    case HTMLFlag.TextFormat:
                        currentStateType.UpdateStyle(tag);
                        break;

                    case HTMLFlag.Element:
                        textBlock.Inlines.Add(UpdateElement(tag, textBlock, currentStateType));
                        break;
                }
            }

            void AddTag(HtmlTag tag)
            {
                while (!previousNode.CanAdd(tag))
                {
                    previousNode = previousNode.Parent;
                }

                previousNode = previousNode.Add(tag);
            }
        }

        private static IEnumerable<HtmlTag> GetTags(this HtmlTagNode node)
        {
            yield return node.Tag;

            foreach (HtmlTagNode subnode in node)
            {
                foreach (var tag in subnode.GetTags())
                {
                    yield return tag;
                }
            }
        }

        /// <summary>
        /// Parse a string and return text before a tag, the tag and it's variables, and the string after that tag.
        /// </summary>
        private static void ReadNextTag(string input, ref string beforeTag, ref string afterTag, ref string tagName, ref string tagVars)
        {
            var pos1 = input.IndexOf('<');
            var pos2 = input.IndexOf('>');

            if ((pos1 == -1) || (pos2 == -1) || (pos2 < pos1))
            {
                tagVars = string.Empty;
                beforeTag = input;
                afterTag = string.Empty;
            }
            else
            {
                var tagStr = input.Substring(pos1 + 1, pos2 - pos1 - 1);
                beforeTag = input.Substring(0, pos1);
                afterTag = input.Substring(pos2 + 1, input.Length - pos2 - 1);

                var pos3 = tagStr.IndexOf(' ');
                if ((pos3 != -1) && (tagStr != string.Empty))
                {
                    tagName = tagStr.Substring(0, pos3);
                    tagVars = tagStr.Substring(pos3 + 1, tagStr.Length - pos3 - 1);
                }
                else
                {
                    tagName = tagStr;
                    tagVars = string.Empty;
                }

                if (!tagName.StartsWith("!--"))
                {
                    return;
                }

                if ((tagName.Length < 6) || (!tagName.EndsWith("--")))
                {
                    var pos4 = afterTag.IndexOf("-->");
                    if (pos4 != -1)
                    {
                        afterTag = afterTag.Substring(pos4 + 2, afterTag.Length - pos4 - 1);
                    }
                }

                tagName = string.Empty;
                tagVars = string.Empty;
            }
        }

        private static Inline UpdateElement(HtmlTag aTag, TextBlock textBlock, CurrentStateType currentStateType)
        {
            Inline retVal;
            switch (aTag.Name)
            {
                case "binding":
                case "text":
                    if (aTag.Name == "binding")
                    {
                        retVal = new Bold(new Run("{Binding}"));
                        if (aTag.Contains("path") && (textBlock.DataContext != null))
                        {
                            var obj = textBlock.DataContext;
                            var pi = obj.GetType().GetProperty(aTag["path"]);

                            if (pi?.CanRead == true)
                            {
                                retVal = new Run(pi.GetValue(obj, null).ToString());
                            }
                        }
                    }
                    else
                    {
                        retVal = new Run(aTag["value"]);
                    }

                    if (currentStateType.SubScript)
                    {
                        retVal.SetValue(Typography.VariantsProperty, FontVariants.Subscript);
                    }

                    if (currentStateType.SuperScript)
                    {
                        retVal.SetValue(Typography.VariantsProperty, FontVariants.Superscript);
                    }

                    if (currentStateType.Bold)
                    {
                        retVal = new Bold(retVal);
                    }

                    if (currentStateType.Italic)
                    {
                        retVal = new Italic(retVal);
                    }

                    if (currentStateType.Underline)
                    {
                        retVal = new Underline(retVal);
                    }

                    if (currentStateType.Foreground.HasValue)
                    {
                        retVal.Foreground = new SolidColorBrush(currentStateType.Foreground.Value);
                    }
                    break;

                case "br":
                    retVal = new LineBreak();
                    break;

                default:
                    retVal = new Run();
                    break;
            }

            if (string.IsNullOrEmpty(currentStateType.HyperLink))
            {
                return retVal;
            }

            var link = new Hyperlink(retVal);
            try
            {
                var url = currentStateType.HyperLink.Trim('\"');
                link.NavigateUri = new Uri(url);
                link.ToolTip = url;
            }
            catch
            {
                link.NavigateUri = null;
            }

            return link;
        }

        private sealed class CurrentStateType
        {
            private readonly List<HtmlTag> _activeStyle = new List<HtmlTag>();

            public bool Bold { get; private set; }
            public bool Italic { get; private set; }
            public bool Underline { get; private set; }
            public bool SubScript { get; private set; }
            public bool SuperScript { get; private set; }
            public string HyperLink { get; private set; }
            public Color? Foreground { get; private set; }

            public void UpdateStyle(HtmlTag aTag)
            {
                if (!aTag.IsEndTag)
                {
                    _activeStyle.Add(aTag);
                }
                else
                {
                    for (var i = _activeStyle.Count - 1; i >= 0; i--)
                    {
                        if ('/' + _activeStyle[i].Name == aTag.Name)
                        {
                            _activeStyle.RemoveAt(i);
                            break;
                        }
                    }
                }

                Bold = false;
                Italic = false;
                Underline = false;
                SubScript = false;
                SuperScript = false;
                Foreground = null;
                HyperLink = "";

                foreach (var tag in _activeStyle)
                {
                    switch (tag.Name)
                    {
                        case "b":
                            Bold = true;
                            break;

                        case "i":
                            Italic = true;
                            break;

                        case "u":
                            Underline = true;
                            break;

                        case "sub":
                            SubScript = true;
                            break;

                        case "sup":
                            SuperScript = true;
                            break;

                        case "a":
                            if (tag.Contains("href"))
                            {
                                HyperLink = tag["href"];
                            }

                            break;
                    }
                }
            }
        }
    }
}
