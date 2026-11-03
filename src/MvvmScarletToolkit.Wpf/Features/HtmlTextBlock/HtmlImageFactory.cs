using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Creates the inline for an html <c>img</c> tag.
    /// <list type="bullet">
    /// <item>Reads <c>src</c> (or <c>source</c>), <c>width</c>, <c>height</c>, <c>alt</c>, <c>title</c> and the <c>style</c> properties width, height, margin, vertical-align and opacity.</item>
    /// <item>Loads http(s), file and relative sources through an <see cref="IImageService{T}"/> when one is available, everything else through a <see cref="BitmapImage"/>.</item>
    /// <item>Shows the <c>alt</c> text, when the image cannot be loaded.</item>
    /// </list>
    /// </summary>
    internal static class HtmlImageFactory
    {
        public static Inline Create(HtmlTag tag, TextBlock textBlock)
        {
            var alt = tag.Get("alt");

            var source = tag.Get("src") ?? tag.Get("source");
            if (string.IsNullOrWhiteSpace(source) || !Uri.TryCreate(source.Trim(), UriKind.RelativeOrAbsolute, out var uri))
            {
                return CreateAltText(alt);
            }

            uri = HtmlTextBlock.Resolve(textBlock, uri);

            var style = InlineStyle.Parse(tag.Get("style"));

            var width = GetLength(tag, style, "width");
            var height = GetLength(tag, style, "height");

            var image = new Image
            {
                Width = width,
                Height = height,
            };

            if (style.TryGetThickness("margin", out var margin))
            {
                image.Margin = margin;
            }

            if (style.TryGetOpacity("opacity", out var opacity))
            {
                image.Opacity = opacity;
            }

            var title = tag.Get("title");
            if (!string.IsNullOrEmpty(title))
            {
                image.ToolTip = title;
            }

            if (!string.IsNullOrEmpty(alt))
            {
                AutomationProperties.SetName(image, alt);
            }

            var container = new InlineUIContainer(image);
            if (style.TryGetBaselineAlignment("vertical-align", out var alignment))
            {
                container.BaselineAlignment = alignment;
            }

            var service = (textBlock as HtmlTextBlock)?.ImageService ?? ImageLoader.AsyncImageLoader;
            if (service is not null && CanUseImageService(uri))
            {
                LoadWithImageService(image, container, service, uri, width, height, alt);

                return container;
            }

            return LoadWithBitmapImage(image, container, uri, alt) ? container : CreateAltText(alt);
        }

        /// <summary>
        /// The image service reads http(s) uris and files, other schemes like pack:// need a custom image data provider.
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

        private static void LoadWithImageService(Image image, InlineUIContainer container, Lazy<IImageService<BitmapSource>> service, Uri uri, double width, double height, string? alt)
        {
            var size = AsyncImageLoad.GetSize(ToDecodeSize(width), ToDecodeSize(height));

            // the load is kept alive by the Loaded and Unloaded handlers it attaches to the image
            var load = new AsyncImageLoad(
                image,
                () => service,
                () => uri,
                () => size,
                state =>
                {
                    if (state == ImageLoadState.Failed)
                    {
                        ShowAltText(container, alt);
                    }
                });

            load.Invalidate();
        }

        private static bool LoadWithBitmapImage(Image image, InlineUIContainer container, Uri uri, string? alt)
        {
            try
            {
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = uri;
                bitmap.EndInit();

                // web images are downloaded after EndInit returned, so their failures are reported later
                bitmap.DownloadFailed += (_, _) => ShowAltText(container, alt);
                bitmap.DecodeFailed += (_, _) => ShowAltText(container, alt);

                image.Source = bitmap;

                return true;
            }
            catch (Exception ex)
            {
                // missing files, unsupported formats and invalid uris must not break the whole text
                Trace.TraceWarning("Loading image from {0} failed: {1}", uri, ex.Message);

                return false;
            }
        }

        private static void ShowAltText(InlineUIContainer container, string? alt)
        {
            container.Child = string.IsNullOrEmpty(alt)
                ? null
                : new TextBlock { Text = alt };
        }

        private static Inline CreateAltText(string? alt)
        {
            return new Run(alt ?? string.Empty);
        }

        private static double GetLength(HtmlTag tag, InlineStyle style, string property)
        {
            // like in browsers, the style wins over the attribute
            if (style.TryGetLength(property, out var length))
            {
                return length;
            }

            return InlineStyle.TryParseLength(tag.Get(property), out length)
                ? length
                : double.NaN;
        }

        private static uint? ToDecodeSize(double length)
        {
            if (double.IsNaN(length) || length < 1 || length > uint.MaxValue)
            {
                return null;
            }

            return (uint)Math.Ceiling(length);
        }
    }
}
