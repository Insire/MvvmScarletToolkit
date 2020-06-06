using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Represent an element Tag in Html
    /// </summary>
    [DebuggerDisplay("{ID} {Name}")]
    internal sealed class HtmlTag
    {
        private readonly IDictionary<string, string> _variables = new Dictionary<string, string>();

        /// <summary>
        /// HtmlTag ID in BuiltInTags. (without <>)
        /// </summary>
        public int ID => HtmlParser.BuiltinTags.ToList().FindIndex(tagInfo => tagInfo.Html.Equals(Name.TrimStart('/')));

        /// <summary>
        /// HtmlTag Level in BuiltInTags. (without <>)
        /// </summary>
        internal int Level => ID == -1
            ? 0
            : HtmlParser.BuiltinTags[ID].TagLevel;

        internal bool IsEndTag => (Name.IndexOf('/') == 0) || _variables.ContainsKey("/");

        /// <summary>
        /// HtmlTag name. (without <>)
        /// </summary>
        public string Name { get; }

        public HtmlTag(IParamParser paramParser, string name, string variableString)
            : this(name, paramParser.StringToDictionary(variableString))
        {
        }

        public HtmlTag(string text)
            : this("text", new Dictionary<string, string> { { "value", text } })
        {
        }

        private HtmlTag(string name, Dictionary<string, string> variables)
        {
            Name = name.ToLower();

            _variables = variables;
        }

        public bool Contains(string key)
        {
            return _variables.ContainsKey(key);
        }

        public string this[string key]
        {
            get { return _variables[key]; }
        }
    }
}
