using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using MvvmScarletToolkit.Html;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// A <see cref="TextBlock"/> that displays formatted text, links and images from a small subset of html in <see cref="Html"/>.
    /// </summary>
    /// <remarks>
    /// <code>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</code>
    /// </remarks>
    public class HtmlTextBlock : TextBlock
    {
        /// <summary>font size of h1 to h6, relative to the font size of the text block</summary>
        private static readonly double[] _headingScales = [2.0, 1.5, 1.25, 1.1, 1.0, 0.9];

        /// <summary>font size of sub and sup, relative to the font size of the text around them</summary>
        private const double ScriptScale = 0.8;

        internal static readonly FontFamily Monospace = new FontFamily("Consolas, Menlo, DejaVu Sans Mono, Courier New");

        public static readonly StyledProperty<string?> HtmlProperty = AvaloniaProperty.Register<HtmlTextBlock, string?>(nameof(Html), string.Empty);

        public static readonly StyledProperty<Uri?> BaseUriProperty = AvaloniaProperty.Register<HtmlTextBlock, Uri?>(nameof(BaseUri));

        public static readonly StyledProperty<bool> OpenLinksInBrowserProperty = AvaloniaProperty.Register<HtmlTextBlock, bool>(nameof(OpenLinksInBrowser));

        public static readonly StyledProperty<Lazy<IImageService<Bitmap>>?> ImageServiceProperty = AvaloniaProperty.Register<HtmlTextBlock, Lazy<IImageService<Bitmap>>?>(nameof(ImageService));

        /// <summary>Whether the last render used the font size, e.g. for headings.</summary>
        private bool _dependsOnFontSize;

        /// <summary>Whether the last render read the data context, through a binding tag.</summary>
        private bool _dependsOnDataContext;

        public HtmlTextBlock()
        {
            LinkCommand = new OpenLinkCommand(this);
        }

        public string? Html
        {
            get => GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }

        /// <summary>
        /// Resolves relative image sources and links, like the location of a page in a browser.
        /// The uri of the html file itself works, e.g. <c>file:///C:/docs/readme.html</c> resolves <c>images/logo.png</c> to <c>C:/docs/images/logo.png</c>.
        /// A folder needs a trailing slash. Relative uris stay as they are while this is <see langword="null"/> or not absolute.
        /// </summary>
        public Uri? BaseUri
        {
            get => GetValue(BaseUriProperty);
            set => SetValue(BaseUriProperty, value);
        }

        /// <summary>
        /// Opens clicked links with the default application of the operating system, usually the browser.
        /// Only http, https and mailto links are opened, since the html may come from an untrusted source.
        /// </summary>
        public bool OpenLinksInBrowser
        {
            get => GetValue(OpenLinksInBrowserProperty);
            set => SetValue(OpenLinksInBrowserProperty, value);
        }

        /// <summary>
        /// Loads and caches the images of <c>img</c> tags with http(s), file and relative sources.
        /// Falls back to <see cref="AsyncImageLoadingBehavior.Loader"/> when not set.
        /// Without either, files and avares:// assets are loaded directly and web images show their alt text.
        /// </summary>
        public Lazy<IImageService<Bitmap>>? ImageService
        {
            get => GetValue(ImageServiceProperty);
            set => SetValue(ImageServiceProperty, value);
        }

        /// <summary>The command of every link while <see cref="OpenLinksInBrowser"/> is set, the uri is passed as parameter.</summary>
        internal ICommand LinkCommand { get; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HtmlProperty
                || change.Property == BaseUriProperty
                || change.Property == OpenLinksInBrowserProperty
                || change.Property == ImageServiceProperty
                || (change.Property == FontSizeProperty && _dependsOnFontSize)
                || (change.Property == DataContextProperty && _dependsOnDataContext))
            {
                Render();
            }
        }

        /// <summary>
        /// Rebuilds the inlines from <see cref="Html"/>.
        /// </summary>
        protected void Render()
        {
            _dependsOnFontSize = false;
            _dependsOnDataContext = false;

            var inlines = new InlineCollection();
            var html = PrepareHtml(Html ?? string.Empty);
            if (!string.IsNullOrEmpty(html))
            {
                AddInlines(inlines, html);
            }

            Inlines = inlines;
        }

        /// <summary>
        /// Changes the html before it is parsed, e.g. to highlight text.
        /// </summary>
        protected virtual string PrepareHtml(string html)
        {
            return html;
        }

        /// <summary>
        /// Combines a relative <paramref name="uri"/> with <see cref="BaseUri"/>, when it is absolute.
        /// </summary>
        internal Uri Resolve(Uri uri)
        {
            if (uri.IsAbsoluteUri || BaseUri is not { IsAbsoluteUri: true } baseUri)
            {
                return uri;
            }

            return Uri.TryCreate(baseUri, uri, out var resolved) ? resolved : uri;
        }

        /// <summary>
        /// Whether <paramref name="uri"/> is a web or mail link. Other schemes like file: could start any program.
        /// </summary>
        internal static bool IsWebLink(Uri? uri)
        {
            return uri is { IsAbsoluteUri: true }
                && (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals(Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase));
        }

        private void AddInlines(InlineCollection inlines, string html)
        {
            var tree = new HtmlTagTree();
            tree.BuildFrom(html);

            var context = new InlineCreationContext<Color>(Color.TryParse);

            foreach (var tag in tree.GetTags())
            {
                switch (tag.Kind)
                {
                    case HtmlTagKind.TextFormat:
                        context.UpdateStyle(tag);
                        break;

                    case HtmlTagKind.Element:
                        inlines.Add(CreateInline(tag, context));
                        break;

                    case HtmlTagKind.List:
                        if (context.UpdateList(tag) is { } marker)
                        {
                            // the marker is formatted like text at its position
                            inlines.Add(CreateInline(HtmlTag.CreateText(marker), context));
                        }

                        break;
                }
            }
        }

        private Inline CreateInline(HtmlTag tag, InlineCreationContext<Color> context)
        {
            switch (tag.Name)
            {
                case "br":
                    return new LineBreak();

                case "text":
                    return WrapInLink(CreateRun(tag.Get("value") ?? string.Empty, context), context.HyperLink);

                case "binding":
                    return WrapInLink(CreateRun(ReadBinding(tag), context), context.HyperLink);

                case "img":
                    var image = HtmlImageFactory.Create(tag, this);

                    // the href of the image wins over a surrounding link
                    return WrapInLink(image, tag.Get("href") ?? context.HyperLink);

                default:
                    return new Run();
            }
        }

        /// <summary>
        /// Reads the property named by the <c>path</c> attribute from the data context, or shows <c>{Binding}</c>.
        /// </summary>
        private string ReadBinding(HtmlTag tag)
        {
            _dependsOnDataContext = true;

            if (tag.Get("path") is { } path
                && DataContext is { } dataContext
                && dataContext.GetType().GetProperty(path) is { CanRead: true } property)
            {
                return property.GetValue(dataContext, null)?.ToString() ?? string.Empty;
            }

            return "{Binding}";
        }

        private Run CreateRun(string text, InlineCreationContext<Color> context)
        {
            var run = new Run(text);

            if (context.Bold)
            {
                run.FontWeight = FontWeight.Bold;
            }

            if (context.Italic)
            {
                run.FontStyle = FontStyle.Italic;
            }

            if (context.Underline)
            {
                run.TextDecorations = global::Avalonia.Media.TextDecorations.Underline;
            }

            if (context.Foreground is { } foreground)
            {
                run.Foreground = new SolidColorBrush(foreground);
            }

            if (context.Monospace)
            {
                run.FontFamily = Monospace;
            }

            var fontSize = FontSize;
            if (context.HeadingLevel > 0)
            {
                _dependsOnFontSize = true;
                fontSize *= _headingScales[context.HeadingLevel - 1];
                run.FontSize = fontSize;
            }

            if (context.SubScript || context.SuperScript)
            {
                _dependsOnFontSize = true;
                run.FontSize = fontSize * ScriptScale;
                run.BaselineAlignment = context.SubScript ? BaselineAlignment.Subscript : BaselineAlignment.Superscript;
            }

            return run;
        }

        /// <summary>
        /// Avalonia has no hyperlink inline, so links are a <see cref="HyperlinkButton"/> in an <see cref="InlineUIContainer"/>.
        /// </summary>
        private Inline WrapInLink(Inline content, string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return content;
            }

            var link = new HyperlinkButton
            {
                Padding = default,
                Margin = default,
                BorderThickness = default,
                MinHeight = 0,
                MinWidth = 0,
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Top,
                Content = CreateLinkContent(content),
            };

            var container = new InlineUIContainer(link);
            if (content is InlineUIContainer image)
            {
                container.BaselineAlignment = image.BaselineAlignment;
            }
            else
            {
                // text in a link sits on the baseline of the text around it
                _dependsOnFontSize = true;
                link.SetValue(BaselineOffsetProperty, GetBaseline(content as Run));
            }

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                var resolved = Resolve(uri);

                // a resolved relative link shows where it leads, other links show what was written
                ToolTip.SetTip(link, ReferenceEquals(resolved, uri) ? url : resolved.AbsoluteUri);

                if (OpenLinksInBrowser)
                {
                    link.Command = LinkCommand;
                    link.CommandParameter = resolved;
                }
            }

            return container;
        }

        private Control CreateLinkContent(Inline content)
        {
            if (content is InlineUIContainer { Child: { } child } image)
            {
                // a control can only have one parent, the container is discarded
                image.Child = null!;

                return child;
            }

            var textBlock = new TextBlock
            {
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                TextDecorations = global::Avalonia.Media.TextDecorations.Underline,
            };
            textBlock.Inlines!.Add(content);

            return textBlock;
        }

        /// <summary>
        /// Distance from the top of the link to the baseline of its text.
        /// </summary>
        private double GetBaseline(Run? run)
        {
            var fontFamily = run?.FontFamily ?? FontFamily;
            var fontSize = run?.IsSet(Run.FontSizeProperty) == true ? run.FontSize : FontSize;
            var typeface = new Typeface(fontFamily, run?.FontStyle ?? FontStyle, run?.FontWeight ?? FontWeight);

            using var layout = new TextLayout(run?.Text ?? " ", typeface, fontSize, null);

            return layout.Baseline;
        }

        private sealed class OpenLinkCommand : ICommand
        {
            private readonly HtmlTextBlock _owner;

            public OpenLinkCommand(HtmlTextBlock owner)
            {
                _owner = owner;
            }

            event EventHandler? ICommand.CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter)
            {
                return IsWebLink(parameter as Uri);
            }

            public async void Execute(object? parameter)
            {
                if (parameter is not Uri uri || !IsWebLink(uri) || TopLevel.GetTopLevel(_owner)?.Launcher is not { } launcher)
                {
                    return;
                }

                try
                {
                    await launcher.LaunchUriAsync(uri);
                }
                catch (Exception ex)
                {
                    // async void, an escaping exception would crash the application
                    Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(_owner, "Opening {Uri} failed: {Exception}", uri, ex);
                }
            }
        }
    }
}
