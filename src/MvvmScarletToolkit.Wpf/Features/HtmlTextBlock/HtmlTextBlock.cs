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
