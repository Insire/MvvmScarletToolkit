using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    public class HtmlTextBlock : TextBlock
    {
        /// <summary>Identifies the <see cref="Html"/> dependency property.</summary>
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
            nameof(Html),
            typeof(string),
            typeof(HtmlTextBlock),
            new PropertyMetadata(string.Empty, OnHtmlChanged));

        public string Html
        {
            get => (string)GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }

        /// <summary>Identifies the <see cref="BaseUri"/> dependency property.</summary>
        public static readonly DependencyProperty BaseUriProperty = DependencyProperty.Register(
            nameof(BaseUri),
            typeof(Uri),
            typeof(HtmlTextBlock),
            new PropertyMetadata(null, OnBaseUriChanged));

        /// <summary>
        /// Resolves relative image sources and links, like the location of a page in a browser.
        /// The uri of the html file itself works, e.g. <c>file:///C:/docs/readme.html</c> resolves <c>images/logo.png</c> to <c>C:/docs/images/logo.png</c>.
        /// A folder needs a trailing slash. Relative uris stay as they are while this is <see langword="null"/> or not absolute.
        /// </summary>
        public Uri? BaseUri
        {
            get => (Uri?)GetValue(BaseUriProperty);
            set => SetValue(BaseUriProperty, value);
        }

        /// <summary>Identifies the <see cref="OpenLinksInBrowser"/> dependency property.</summary>
        public static readonly DependencyProperty OpenLinksInBrowserProperty = DependencyProperty.Register(
            nameof(OpenLinksInBrowser),
            typeof(bool),
            typeof(HtmlTextBlock),
            new PropertyMetadata(false, OnOpenLinksInBrowserChanged));

        /// <summary>
        /// Opens clicked links with the default application of the operating system, usually the browser.
        /// Only http, https and mailto links are opened, since the html may come from an untrusted source.
        /// </summary>
        public bool OpenLinksInBrowser
        {
            get => (bool)GetValue(OpenLinksInBrowserProperty);
            set => SetValue(OpenLinksInBrowserProperty, value);
        }

        /// <summary>Identifies the <see cref="ImageService"/> dependency property.</summary>
        public static readonly DependencyProperty ImageServiceProperty = DependencyProperty.Register(
            nameof(ImageService),
            typeof(Lazy<IImageService<BitmapSource>>),
            typeof(HtmlTextBlock),
            new PropertyMetadata(null, OnImageServiceChanged));

        /// <summary>
        /// Loads and caches the images of <c>img</c> tags with http(s), file and relative sources.
        /// Falls back to <see cref="ImageLoader.AsyncImageLoader"/> when not set.
        /// Without either, and for other sources like pack uris, images are loaded with a <see cref="BitmapImage"/>.
        /// </summary>
        public Lazy<IImageService<BitmapSource>>? ImageService
        {
            get => (Lazy<IImageService<BitmapSource>>?)GetValue(ImageServiceProperty);
            set => SetValue(ImageServiceProperty, value);
        }

        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        protected virtual void Parse(string html)
        {
            Inlines.Clear();
            if (string.IsNullOrEmpty(html))
            {
                return;
            }

            this.UpdateWith(html);
        }

        private static void OnHtmlChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlTextBlock textBlock)
            {
                // null clears the inlines as well
                textBlock.Parse(e.NewValue as string ?? string.Empty);
            }
        }

        private static void OnBaseUriChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlTextBlock textBlock)
            {
                textBlock.Parse(textBlock.Html ?? string.Empty);
            }
        }

        /// <summary>
        /// Combines a relative <paramref name="uri"/> with the <see cref="BaseUri"/> of <paramref name="textBlock"/>, when it has an absolute one.
        /// </summary>
        internal static Uri Resolve(TextBlock textBlock, Uri uri)
        {
            if (uri.IsAbsoluteUri || textBlock is not HtmlTextBlock { BaseUri: { IsAbsoluteUri: true } baseUri })
            {
                return uri;
            }

            return Uri.TryCreate(baseUri, uri, out var resolved) ? resolved : uri;
        }

        private static void OnOpenLinksInBrowserChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlTextBlock textBlock)
            {
                textBlock.Parse(textBlock.Html ?? string.Empty);
            }
        }

        private static void OnImageServiceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlTextBlock textBlock)
            {
                textBlock.Parse(textBlock.Html ?? string.Empty);
            }
        }
    }
}
