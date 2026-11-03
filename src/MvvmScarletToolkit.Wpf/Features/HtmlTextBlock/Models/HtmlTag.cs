using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Represent an element Tag in Html
    /// </summary>
    [DebuggerDisplay("{Id} {Name}")]
    internal sealed class HtmlTag
    {
        /// <summary>font size of h1 to h6, relative to the font size of the text block</summary>
        private static readonly double[] _headingScales = [2.0, 1.5, 1.25, 1.1, 1.0, 0.9];

        private static readonly FontFamily _monospace = new FontFamily("Consolas, Courier New, monospace");

        /// <summary>shared by all links, the uri is passed as command parameter</summary>
        private static readonly RelayCommand<Uri?> _openLinkCommand = new RelayCommand<Uri?>(
            uri => ShellExecute.Start(ShellExecute.CreateStartInfo(uri!)),
            ShellExecute.IsWebLink);

        internal static ICommand OpenLinkCommand => _openLinkCommand;

        private readonly Dictionary<string, string> _variables;

        /// <summary>
        /// HtmlTag Id in BuiltInTags. (without &lt;&gt;)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// HtmlTag name. (without &lt;&gt;)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the tag that closes this one, e.g. <c>/b</c> for <c>b</c>.
        /// </summary>
        internal string EndTagName { get; }

        /// <summary>
        /// HtmlTag Level in BuiltInTags. (without &lt;&gt;)
        /// </summary>
        internal int Level { get; }

        internal bool IsEndTag { get; }

        private HtmlTag(string name, Dictionary<string, string> variables)
        {
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));

            // invariant, otherwise <I> would become <ı> in turkish and no longer be recognized
            Name = name.ToLowerInvariant();
            EndTagName = '/' + Name;

            Id = HtmlParser.GetTagIndex(Name.TrimStart('/'));
            IsEndTag = Name.StartsWith('/') || _variables.ContainsKey("/");
            Level = Id == -1 ? 0 : HtmlParser.GetTagInfo(Id).TagLevel;
        }

        public HtmlTag(IParamParser paramParser, string name, string variableString)
            : this(name, paramParser.StringToDictionary(variableString))
        {
        }

        public HtmlTag(string text)
            : this("text", new Dictionary<string, string> { { "value", WebUtility.HtmlDecode(text) } })
        {
        }

        public bool Contains(string key)
        {
            return _variables.ContainsKey(key);
        }

        public string this[string key] => _variables[key];

        /// <summary>
        /// Returns the value of an attribute, or <see langword="null"/> when the tag does not have it.
        /// </summary>
        public string? Get(string key)
        {
            return _variables.TryGetValue(key, out var value) ? value : null;
        }

        public Inline CreateInline(TextBlock textBlock, InlineCreationContext currentStateType)
        {
            var result = CreateInlineInternal(textBlock, currentStateType);

            return TryWrapInHyperLink(textBlock, result, currentStateType.HyperLink);
        }

        private Inline CreateInlineInternal(TextBlock textBlock, InlineCreationContext currentStateType)
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

                            if (property?.CanRead == true)
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

                    if (currentStateType.HeadingLevel > 0)
                    {
                        // relative to the font size at the time the html is parsed
                        result.FontSize = textBlock.FontSize * _headingScales[currentStateType.HeadingLevel - 1];
                    }

                    if (currentStateType.Monospace)
                    {
                        result.FontFamily = _monospace;
                    }

                    return result;

                case "img":
                    var image = HtmlImageFactory.Create(this, textBlock);

                    return Contains("href")
                        ? TryWrapInHyperLink(textBlock, image, _variables["href"])
                        : image;

                default:
                    return new Run();
            }
        }

        private static Inline TryWrapInHyperLink(TextBlock textBlock, Inline content, string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return content;
            }

            var link = new Hyperlink(content);

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                var resolved = HtmlTextBlock.Resolve(textBlock, uri);

                link.NavigateUri = resolved;

                // a resolved relative link shows where it leads, other links show what was written
                link.ToolTip = ReferenceEquals(resolved, uri) ? url : resolved.AbsoluteUri;
                uri = resolved;

                if (textBlock is HtmlTextBlock { OpenLinksInBrowser: true })
                {
                    link.Command = _openLinkCommand;
                    link.CommandParameter = uri;
                }
            }
            else
            {
                link.NavigateUri = null;
            }

            return link;
        }
    }
}
