using System.Collections.Generic;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf
{
    internal static partial class HtmlParser
    {
        private static readonly List<HTMLTagInfo> _builtinTags = new List<HTMLTagInfo>(51)
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
            var tree = new HtmlTagTree(paramParser, _builtinTags);

            tree.BuildFrom(htmlInput);

            // update textbox with inline elements according to syntax tree items
            var context = new InlineCreationContext();

            foreach (var tag in tree.GetTags())
            {
                switch (_builtinTags[tag.ID].Flags)
                {
                    case HTMLFlag.TextFormat:
                        context.UpdateStyle(tag);
                        break;

                    case HTMLFlag.Element:
                        var inline = tag.CreateInline(textBlock, context);
                        textBlock.Inlines.Add(inline);
                        break;
                }
            }
        }
    }
}
