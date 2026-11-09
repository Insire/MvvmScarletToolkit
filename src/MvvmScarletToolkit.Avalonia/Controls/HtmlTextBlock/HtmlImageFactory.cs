using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MvvmScarletToolkit.Html;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Creates the inline for an html <c>img</c> tag.
    /// <list type="bullet">
    /// <item>Reads <c>src</c> (or <c>source</c>), <c>width</c>, <c>height</c>, <c>alt</c>, <c>title</c> and the <c>style</c> properties width, height, margin, vertical-align and opacity.</item>
    /// <item>Loads http(s), file and relative sources through an <see cref="IImageService{T}"/> when one is available.
    /// Without one, files and avares:// assets are loaded directly.</item>
    /// <item>Shows the <c>alt</c> text, when the image cannot be loaded.</item>
    /// </list>
    /// </summary>
    internal static class HtmlImageFactory
    {
        public static Inline Create(HtmlTag tag, HtmlTextBlock owner)
        {
            var alt = tag.Get("alt");

            var source = tag.Get("src") ?? tag.Get("source");
            if (string.IsNullOrWhiteSpace(source) || !Uri.TryCreate(source.Trim(), UriKind.RelativeOrAbsolute, out var uri))
            {
                return CreateAltText(alt);
            }

            uri = owner.Resolve(uri);

            var style = InlineStyle.Parse(tag.Get("style"));

            var width = InlineStyle.GetLength(tag, style, "width");
            var height = InlineStyle.GetLength(tag, style, "height");

            var image = CreateImage(tag, style, width, height, alt);

            // the host swaps the image for its alt text, even after the image was moved into a link
            var host = new Decorator { Child = image };
            var container = new InlineUIContainer(host);
            if (style.TryGetVerticalAlign("vertical-align", out var alignment))
            {
                container.BaselineAlignment = ToBaselineAlignment(alignment);
            }

            var service = owner.ImageService ?? AsyncImageLoadingBehavior.Loader;
            if (service is not null && CanUseImageService(uri))
            {
                LoadWithImageService(image, host, service, uri, width, height, alt);

                return container;
            }

            if (TryLoadDirectly(uri) is { } bitmap)
            {
                image.Source = bitmap;

                return container;
            }

            return CreateAltText(alt);
        }

        private static Image CreateImage(HtmlTag tag, InlineStyle style, double width, double height, string? alt)
        {
            var image = new Image
            {
                Width = width,
                Height = height,
            };

            if (style.TryGetThickness("margin", out var margin))
            {
                image.Margin = new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);
            }

            if (style.TryGetOpacity("opacity", out var opacity))
            {
                image.Opacity = opacity;
            }

            var title = tag.Get("title");
            if (!string.IsNullOrEmpty(title))
            {
                ToolTip.SetTip(image, title);
            }

            if (!string.IsNullOrEmpty(alt))
            {
                AutomationProperties.SetName(image, alt);
            }

            return image;
        }

        /// <summary>
        /// The image service reads http(s) uris and files, other schemes like avares:// need a custom image data provider.
        /// </summary>
        internal static bool CanUseImageService(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return true;
            }

            return uri.IsFile
                || uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }

        private static void LoadWithImageService(Image image, Decorator host, Lazy<IImageService<Bitmap>> service, Uri uri, double width, double height, string? alt)
        {
            var decodeWidth = InlineStyle.ToDecodeSize(width);
            var decodeHeight = InlineStyle.ToDecodeSize(height);

            image.PropertyChanged += (_, e) =>
            {
                if (e.Property == AsyncImageLoadingBehavior.StateProperty && e.NewValue is ImageLoadState.Failed)
                {
                    ShowAltText(host, alt);
                }
            };

            AsyncImageLoadingBehavior.SetLoader(image, service);
            AsyncImageLoadingBehavior.SetWidth(image, decodeWidth);
            AsyncImageLoadingBehavior.SetHeight(image, decodeHeight);
            AsyncImageLoadingBehavior.SetSource(image, uri);
        }

        /// <summary>
        /// Loads files and avares:// assets without an image service. Web images need one.
        /// </summary>
        private static Bitmap? TryLoadDirectly(Uri uri)
        {
            try
            {
                if (!uri.IsAbsoluteUri)
                {
                    return null;
                }

                if (uri.IsFile)
                {
                    return new Bitmap(uri.LocalPath);
                }

                if (uri.Scheme.Equals("avares", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = AssetLoader.Open(uri);

                    return new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                // missing files, unsupported formats and invalid uris must not break the whole text
                Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(null, "Loading image from {Uri} failed: {Exception}", uri, ex.Message);
            }

            return null;
        }

        private static void ShowAltText(Decorator host, string? alt)
        {
            host.Child = string.IsNullOrEmpty(alt)
                ? null
                : new TextBlock { Text = alt };
        }

        private static Run CreateAltText(string? alt)
        {
            return new Run(alt ?? string.Empty);
        }

        private static BaselineAlignment ToBaselineAlignment(VerticalAlign alignment)
        {
            return alignment switch
            {
                VerticalAlign.Top => BaselineAlignment.Top,
                VerticalAlign.Middle => BaselineAlignment.Center,
                VerticalAlign.Bottom => BaselineAlignment.Bottom,
                VerticalAlign.TextTop => BaselineAlignment.TextTop,
                VerticalAlign.TextBottom => BaselineAlignment.TextBottom,
                _ => BaselineAlignment.Baseline,
            };
        }
    }
}
