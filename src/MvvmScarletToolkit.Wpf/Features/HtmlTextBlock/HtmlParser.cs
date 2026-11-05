using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Html;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Fills the inlines of a <see cref="TextBlock"/> from html, parsed by the shared <see cref="HtmlTagTree"/>.
    /// </summary>
    internal static class HtmlParser
    {
        /// <summary>font size of h1 to h6, relative to the font size of the text block</summary>
        private static readonly double[] _headingScales = [2.0, 1.5, 1.25, 1.1, 1.0, 0.9];

        private static readonly FontFamily _monospace = new FontFamily("Consolas, Courier New, monospace");

        /// <summary>shared by all links, the uri is passed as command parameter</summary>
        private static readonly RelayCommand<Uri?> _openLinkCommand = new RelayCommand<Uri?>(
            uri => ShellExecute.Start(ShellExecute.CreateStartInfo(uri!)),
            ShellExecute.IsWebLink);

        internal static ICommand OpenLinkCommand => _openLinkCommand;

        public static void UpdateWith(this TextBlock textBlock, string htmlInput)
        {
            var tree = new HtmlTagTree();
            tree.BuildFrom(htmlInput);

            var context = new InlineCreationContext<Color>(TryParseColor);

            foreach (var tag in tree.GetTags())
            {
                switch (tag.Kind)
                {
                    case HtmlTagKind.TextFormat:
                        context.UpdateStyle(tag);
                        break;

                    case HtmlTagKind.Element:
                        textBlock.Inlines.Add(CreateInline(tag, textBlock, context));
                        break;

                    case HtmlTagKind.List:
                        if (context.UpdateList(tag) is { } marker)
                        {
                            // the marker is formatted like text at its position
                            textBlock.Inlines.Add(CreateInline(HtmlTag.CreateText(marker), textBlock, context));
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Reads named colors and #rgb, #rrggbb and #aarrggbb.
        /// </summary>
        internal static bool TryParseColor(string value, out Color color)
        {
            color = default;

            try
            {
                if (ColorConverter.ConvertFromString(value) is Color parsed)
                {
                    color = parsed;
                    return true;
                }
            }
            catch (FormatException)
            {
                // invalid colors are ignored
            }
            catch (NotSupportedException)
            {
                // invalid colors are ignored
            }

            return false;
        }

        private static Inline CreateInline(HtmlTag tag, TextBlock textBlock, InlineCreationContext<Color> context)
        {
            var result = CreateInlineInternal(tag, textBlock, context);

            return TryWrapInHyperLink(textBlock, result, context.HyperLink);
        }

        private static Inline CreateInlineInternal(HtmlTag tag, TextBlock textBlock, InlineCreationContext<Color> context)
        {
            switch (tag.Name)
            {
                case "br":
                    return new LineBreak();

                case "binding":
                case "text":
                    return CreateText(tag, textBlock, context);

                case "img":
                    var image = HtmlImageFactory.Create(tag, textBlock);

                    return TryWrapInHyperLink(textBlock, image, tag.Get("href"));

                default:
                    return new Run();
            }
        }

        private static Inline CreateText(HtmlTag tag, TextBlock textBlock, InlineCreationContext<Color> context)
        {
            Inline result;
            if (tag.Name == "binding")
            {
                result = new Bold(new Run("{Binding}"));
                if (tag.Get("path") is { } propertyName
                    && textBlock.DataContext is { } dataContext
                    && dataContext.GetType().GetProperty(propertyName) is { CanRead: true } property)
                {
                    var value = property.GetValue(dataContext, null);
                    result = new Run(value?.ToString() ?? "");
                }
            }
            else
            {
                result = new Run(tag.Get("value"));
            }

            if (context.SubScript)
            {
                result.SetCurrentValue(Typography.VariantsProperty, FontVariants.Subscript);
            }

            if (context.SuperScript)
            {
                result.SetCurrentValue(Typography.VariantsProperty, FontVariants.Superscript);
            }

            if (context.Bold)
            {
                result = new Bold(result);
            }

            if (context.Italic)
            {
                result = new Italic(result);
            }

            if (context.Underline)
            {
                result = new Underline(result);
            }

            if (context.Foreground.HasValue)
            {
                result.Foreground = new SolidColorBrush(context.Foreground.Value);
            }

            if (context.HeadingLevel > 0)
            {
                // relative to the font size at the time the html is parsed
                result.FontSize = textBlock.FontSize * _headingScales[context.HeadingLevel - 1];
            }

            if (context.Monospace)
            {
                result.FontFamily = _monospace;
            }

            return result;
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

                if (textBlock is HtmlTextBlock { OpenLinksInBrowser: true })
                {
                    link.Command = _openLinkCommand;
                    link.CommandParameter = resolved;
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
