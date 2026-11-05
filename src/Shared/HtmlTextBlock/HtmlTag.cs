using System.Collections.Frozen;
using System.Diagnostics;
using System.Net;

namespace MvvmScarletToolkit.Html
{
    internal enum HtmlTagKind
    {
        /// <summary>Not rendered, only used to build the tree.</summary>
        None,

        /// <summary>Changes the format of the text that follows, until the end tag.</summary>
        TextFormat,

        /// <summary>Becomes an inline: text, br, img and binding.</summary>
        Element,

        /// <summary>ul, ol and li, which add bullets or numbers to the text.</summary>
        List,
    }

    /// <summary>
    /// An html tag or a piece of text, with its attributes.
    /// </summary>
    [DebuggerDisplay("{Id} {Name}")]
    internal sealed class HtmlTag
    {
        private sealed record TagInfo(string Name, HtmlTagKind Kind, short Level);

        // level guide, a tag closes every open tag with a lower level
        // 50 master, 40 xml, 30 var, 15 html, 14 head/body/title, 13 div/table, 12 form, 11 tr, 10 td/th,
        // 09 ul/ol, 08 li/blockquote, 07 p/h1-h6/span/pre, 04 text formatting and a, 02 controls, 01 script,
        // 00 text, br, hr, img and binding
        private static readonly TagInfo[] _builtinTags =
        [
            new TagInfo("master",       HtmlTagKind.None,        50),
            new TagInfo("xml",          HtmlTagKind.None,        40),
            new TagInfo("var",          HtmlTagKind.None,        30),
            new TagInfo("html",         HtmlTagKind.None,        15),
            new TagInfo("head",         HtmlTagKind.None,        14),
            new TagInfo("body",         HtmlTagKind.None,        14),
            new TagInfo("title",        HtmlTagKind.None,        14),
            new TagInfo("div",          HtmlTagKind.None,        13),
            new TagInfo("selection",    HtmlTagKind.TextFormat,  13),
            new TagInfo("hi",           HtmlTagKind.TextFormat,  13),
            new TagInfo("table",        HtmlTagKind.None,        13),
            new TagInfo("centre",       HtmlTagKind.None,        13),
            new TagInfo("form",         HtmlTagKind.None,        12),
            new TagInfo("tr",           HtmlTagKind.None,        11),
            new TagInfo("td",           HtmlTagKind.None,        10),
            new TagInfo("th",           HtmlTagKind.None,        10),
            new TagInfo("ul",           HtmlTagKind.List,        09),
            new TagInfo("ol",           HtmlTagKind.List,        09),
            new TagInfo("li",           HtmlTagKind.List,        08),
            new TagInfo("blockquote",   HtmlTagKind.TextFormat,  08),
            new TagInfo("indent",       HtmlTagKind.None,        08),
            new TagInfo("p",            HtmlTagKind.None,        07),
            new TagInfo("h1",           HtmlTagKind.TextFormat,  07),
            new TagInfo("h2",           HtmlTagKind.TextFormat,  07),
            new TagInfo("h3",           HtmlTagKind.TextFormat,  07),
            new TagInfo("h4",           HtmlTagKind.TextFormat,  07),
            new TagInfo("h5",           HtmlTagKind.TextFormat,  07),
            new TagInfo("h6",           HtmlTagKind.TextFormat,  07),
            new TagInfo("span",         HtmlTagKind.TextFormat,  07), // TextFormat, so its style can color the text
            new TagInfo("font",         HtmlTagKind.TextFormat,  04),
            new TagInfo("u",            HtmlTagKind.TextFormat,  04),
            new TagInfo("b",            HtmlTagKind.TextFormat,  04),
            new TagInfo("s",            HtmlTagKind.TextFormat,  04),
            new TagInfo("i",            HtmlTagKind.TextFormat,  04),
            new TagInfo("a",            HtmlTagKind.TextFormat,  04),
            new TagInfo("sup",          HtmlTagKind.TextFormat,  04),
            new TagInfo("sub",          HtmlTagKind.TextFormat,  04),
            new TagInfo("strong",       HtmlTagKind.TextFormat,  04),
            new TagInfo("color",        HtmlTagKind.TextFormat,  04),
            new TagInfo("code",         HtmlTagKind.TextFormat,  04),
            new TagInfo("pre",          HtmlTagKind.TextFormat,  07),
            new TagInfo("input",        HtmlTagKind.None,        02),
            new TagInfo("select",       HtmlTagKind.None,        02),
            new TagInfo("option",       HtmlTagKind.None,        02),
            new TagInfo("script",       HtmlTagKind.None,        01),
            new TagInfo("meta",         HtmlTagKind.None,        00),
            new TagInfo("br",           HtmlTagKind.Element,     00),
            new TagInfo("hr",           HtmlTagKind.Element,     00),
            new TagInfo("img",          HtmlTagKind.Element,     00),
            new TagInfo("text",         HtmlTagKind.Element,     00),
            new TagInfo("binding",      HtmlTagKind.Element,     00),
            new TagInfo("dynamic",      HtmlTagKind.None,        00),
            new TagInfo("user",         HtmlTagKind.None,        00),
        ];

        /// <summary>
        /// Index of each tag name in <see cref="_builtinTags"/>. Immutable, so parsing on several threads at once is safe.
        /// Declared after <see cref="_builtinTags"/>, since static fields are initialized in declaration order.
        /// </summary>
        private static readonly FrozenDictionary<string, int> _tagIndices = _builtinTags
            .Select((tag, index) => (tag.Name, index))
            .ToFrozenDictionary(p => p.Name, p => p.index, StringComparer.Ordinal);

        private readonly Dictionary<string, string> _attributes;

        /// <summary>Index in the built-in tags, -1 for unknown tags.</summary>
        public int Id { get; }

        /// <summary>Lower case name without &lt;&gt;, e.g. <c>b</c> or <c>/b</c>.</summary>
        public string Name { get; }

        /// <summary>The name of the tag that closes this one, e.g. <c>/b</c> for <c>b</c>.</summary>
        public string EndTagName { get; }

        public int Level { get; }

        public HtmlTagKind Kind { get; }

        public bool IsEndTag { get; }

        private HtmlTag(string name, Dictionary<string, string> attributes)
        {
            _attributes = attributes;

            // invariant, otherwise <I> would become <ı> in turkish and no longer be recognized
            Name = name.ToLowerInvariant();
            EndTagName = '/' + Name;

            Id = _tagIndices.TryGetValue(Name.TrimStart('/'), out var index) ? index : -1;
            IsEndTag = Name.StartsWith('/') || _attributes.ContainsKey("/");
            Level = Id == -1 ? 0 : _builtinTags[Id].Level;
            Kind = Id == -1 ? HtmlTagKind.None : _builtinTags[Id].Kind;
        }

        public HtmlTag(string name, string attributes)
            : this(name, HtmlAttributes.Parse(attributes))
        {
        }

        /// <summary>
        /// Creates a text tag, decoding entities like <c>&amp;lt;</c>.
        /// </summary>
        public static HtmlTag CreateText(string text)
        {
            return new HtmlTag("text", new Dictionary<string, string> { { "value", WebUtility.HtmlDecode(text) } });
        }

        public bool Contains(string key)
        {
            return _attributes.ContainsKey(key);
        }

        /// <summary>
        /// Returns the value of an attribute, or <see langword="null"/> when the tag does not have it.
        /// </summary>
        public string? Get(string key)
        {
            return _attributes.TryGetValue(key, out var value) ? value : null;
        }
    }
}
