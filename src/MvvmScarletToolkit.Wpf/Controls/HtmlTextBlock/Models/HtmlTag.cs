using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Represent an element Tag in Html
    /// </summary>
    [DebuggerDisplay("{ID} {Name}")]
    internal sealed class HtmlTag
    {
        private readonly Dictionary<string, string> _variables;

        private readonly Lazy<int> _id;
        private readonly Lazy<bool> _isEndTag;
        private readonly Lazy<int> _level;

        /// <summary>
        /// HtmlTag ID in BuiltInTags. (without <>)
        /// </summary>
        public int ID => _id.Value;

        /// <summary>
        /// HtmlTag name. (without <>)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// HtmlTag Level in BuiltInTags. (without <>)
        /// </summary>
        internal int Level => _level.Value;

        internal bool IsEndTag => _isEndTag.Value;

        private HtmlTag(string name, Dictionary<string, string> variables)
        {
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            Name = name.ToLower();

            _id = new Lazy<int>(() => HtmlParser.BuiltinTags.ToList().FindIndex(tagInfo => tagInfo.Html.Equals(Name.TrimStart('/'))));
            _isEndTag = new Lazy<bool>(() => (Name.IndexOf('/') == 0) || _variables.ContainsKey("/"));
            _level = new Lazy<int>(() => ID == -1 ? 0 : HtmlParser.BuiltinTags[ID].TagLevel);
        }

        public HtmlTag(IParamParser paramParser, string name, string variableString)
            : this(name, paramParser.StringToDictionary(variableString))
        {
        }

        public HtmlTag(string text)
            : this("text", new Dictionary<string, string> { { "value", text } })
        {
        }

        public bool Contains(string key)
        {
            return _variables.ContainsKey(key);
        }

        public string this[string key]
        {
            get { return _variables[key]; }
        }

        public Inline CreateInline(TextBlock textBlock, CurrentStateType currentStateType)
        {
            var result = CreateInlineInternal(textBlock, currentStateType);

            return TryCreateHyperLink(result, currentStateType.HyperLink?.Trim('\"'));
        }

        private Inline CreateInlineInternal(TextBlock textBlock, CurrentStateType currentStateType)
        {
            switch (Name)
            {
                case "br":
                    return new LineBreak();

                case "binding":
                case "text":

                    Inline result;
                    if (Name == "binding")
                    {
                        result = new Bold(new Run("{Binding}"));
                        if (Contains("path") && (textBlock.DataContext != null))
                        {
                            var dataContext = textBlock.DataContext;
                            var propertyName = _variables["path"];
                            var property = dataContext.GetType().GetProperty(propertyName);

                            if (property != null && property.CanRead == true)
                            {
                                var value = property.GetValue(dataContext, null);
                                result = new Run(value?.ToString() ?? "");
                            }
                        }
                    }
                    else
                    {
                        result = new Run(_variables["value"]);
                    }

                    if (currentStateType.SubScript)
                    {
                        result.SetCurrentValue(Typography.VariantsProperty, FontVariants.Subscript);
                    }

                    if (currentStateType.SuperScript)
                    {
                        result.SetCurrentValue(Typography.VariantsProperty, FontVariants.Superscript);
                    }

                    if (currentStateType.Bold)
                    {
                        result = new Bold(result);
                    }

                    if (currentStateType.Italic)
                    {
                        result = new Italic(result);
                    }

                    if (currentStateType.Underline)
                    {
                        result = new Underline(result);
                    }

                    if (currentStateType.Foreground.HasValue)
                    {
                        result.Foreground = new SolidColorBrush(currentStateType.Foreground.Value);
                    }

                    return result;

                case "img":
                    if (Contains("source"))
                    {
                        var width = double.NaN;
                        if (Contains("width") && double.TryParse(_variables["width"], out var internal_width))
                            width = internal_width;

                        var height = double.NaN;
                        if (Contains("height") && double.TryParse(_variables["height"], out var internal_height))
                            height = internal_height;

                        if (!Uri.TryCreate(_variables["source"], UriKind.RelativeOrAbsolute, out var uri))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = uri;
                            bitmap.EndInit();

                            var image = new Image
                            {
                                Source = bitmap,
                                Width = width,
                                Height = height
                            };

                            if (Contains("href"))
                            {
                                return TryCreateHyperLink(new InlineUIContainer(image), _variables["href"]);
                            }

                            return new InlineUIContainer(image);
                        }
                    }

                    return new Run();

                default:
                    return new Run();
            }
        }

        private static Inline TryCreateHyperLink(Inline content, string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return content;
            }

            var link = new Hyperlink(content);

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                link.NavigateUri = uri;
                link.ToolTip = url;
            }
            else
            {
                link.NavigateUri = null;
            }

            return link;
        }
    }
}
