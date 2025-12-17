using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    // original idea and code from: https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia
    public static class ImageLoader
    {
        public static Lazy<IImageService<BitmapSource>> AsyncImageLoader { get; set; } = null!;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(Uri),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnSourceChanged));

        /// <summary>Helper for getting <see cref="SourceProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="SourceProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static Uri? GetSource(FrameworkElement image)
        {
            return (Uri?)image.GetValue(SourceProperty);
        }

        /// <summary>Helper for setting <see cref="SourceProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="FrameworkElement"/> to set <see cref="SourceProperty"/> on.</param>
        /// <param name="value">First property value.</param>
        public static void SetSource(FrameworkElement image, Uri? value)
        {
            image.SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.RegisterAttached(
            "IsLoading",
            typeof(bool),
            typeof(ImageLoader),
            new PropertyMetadata(false));

        /// <summary>Helper for getting <see cref="IsLoadingProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="FrameworkElement"/> to read <see cref="IsLoadingProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetIsLoading(FrameworkElement image)
        {
            return (bool)image.GetValue(IsLoadingProperty);
        }

        /// <summary>Helper for setting <see cref="IsLoadingProperty"/> on <paramref name="frameworkElement"/>.</summary>
        /// <param name="frameworkElement"><see cref="Image"/> to set <see cref="IsLoadingProperty"/> on.</param>
        /// <param name="value">First property value.</param>
        public static void SetIsLoading(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width",
            typeof(uint?),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnWidthChanged));

        /// <summary>Helper for setting <see cref="WidthProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to set <see cref="WidthProperty"/> on.</param>
        public static uint? GetWidth(FrameworkElement image)
        {
            return (uint?)image.GetValue(WidthProperty);
        }

        /// <summary>Helper for getting <see cref="WidthProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="WidthProperty"/> from.</param>
        /// <param name="value"></param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static void SetWidth(FrameworkElement image, uint? value)
        {
            image.SetValue(WidthProperty, value);
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached(
            "Height",
            typeof(uint?),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnHeightChanged));

        /// <summary>Helper for setting <see cref="HeightProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="FrameworkElement"/> to set <see cref="HeightProperty"/> on.</param>
        public static uint? GetHeight(FrameworkElement image)
        {
            return (uint?)image.GetValue(HeightProperty);
        }

        /// <summary>Helper for getting <see cref="HeightProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="HeightProperty"/> from.</param>
        /// <param name="value"></param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static void SetHeight(FrameworkElement image, uint? value)
        {
            image.SetValue(HeightProperty, value);
        }

        private static async void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not Image image)
            {
                return;
            }

            if (e.NewValue is Uri uri)
            {
                await OnChanged(image, uri, GetWidth(image), GetHeight(image));
            }
            else
            {
                await OnChanged(image, null, GetWidth(image), GetHeight(image));
            }
        }

        private static async void OnWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not Image image)
            {
                return;
            }

            if (e.NewValue is uint width)
            {
                await OnChanged(image, GetSource(image), width, GetHeight(image));
            }
            else
            {
                await OnChanged(image, GetSource(image), null, GetHeight(image));
            }
        }

        private static async void OnHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not Image image)
            {
                return;
            }

            if (e.NewValue is uint height)
            {
               await OnChanged(image, GetSource(image), GetWidth(image), height);
            }
            else
            {
                await OnChanged(image, GetSource(image), GetWidth(image), null);
            }
        }

        private static async Task OnChanged(Image sender, Uri? url, uint? width, uint? height)
        {
            if (GetSource(sender) != url)
            {
                return;
            }

            var size = GetSize(width, height);

            sender.Source = await AsyncImageLoader.Value
                .ProvideImageAsync(url, size, async (isloading) => await sender.Dispatcher.BeginInvoke(() => SetIsLoading(sender, isloading)))
                .ConfigureAwait(true);

            SetIsLoading(sender, false);
        }

        private static ImageSize? GetSize(uint? width, uint? height)
        {
            if (height is not null && width is not null)
            {
                return new ImageSize(width.Value, height.Value);
            }

            return null;
        }
    }
}
