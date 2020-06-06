using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;

namespace MvvmScarletToolkit.Wpf
{
    internal static partial class HtmlParser
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
            HtmlTagNode? previousNode = tree;

            var beforeTag = string.Empty;
            var afterTag = string.Empty;
            var name = string.Empty;
            var vars = string.Empty;

            // build syntax tree
            do
            {
                ReadNextTag(htmlInput, ref beforeTag, ref afterTag, ref name, ref vars);

                if (beforeTag != string.Empty)
                {
                    AddTag(new HtmlTag(beforeTag));
                }

                if (name != string.Empty)
                {
                    AddTag(new HtmlTag(paramParser, name, vars));
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
                        textBlock.Inlines.Add(tag.CreateInline(textBlock, currentStateType));
                        break;
                }
            }

            void AddTag(HtmlTag tag)
            {
                while (previousNode != null && !previousNode.CanAdd(tag))
                {
                    previousNode = previousNode?.Parent;
                }

                previousNode = previousNode?.Add(tag);
            }
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
