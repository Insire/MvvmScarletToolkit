using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    // original idea and code from: https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia
    public static class ImageLoader
    {
        public static Lazy<IImageService<BitmapSource>> AsyncImageLoader { get; set; } = default!;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(Uri),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnSourceChanged));

        /// <summary>Helper for getting <see cref="SourceProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="SourceProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.Image))]
        public static Uri? GetSource(FrameworkElement image)
        {
            return (Uri?)image.GetValue(SourceProperty);
        }

        /// <summary>Helper for setting <see cref="FirstProperty"/> on <paramref name="image"/>.</summary>
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
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.Image))]
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
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.Image))]
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
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.Image))]
        public static void SetHeight(FrameworkElement image, uint? value)
        {
            image.SetValue(HeightProperty, value);
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not System.Windows.Controls.Image image)
            {
                return;
            }

            if (e.NewValue is Uri uri)
            {
                OnChanged(image, uri, GetWidth(image), GetHeight(image));
            }
            else
            {
                OnChanged(image, null, GetWidth(image), GetHeight(image));
            }
        }

        private static void OnWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not System.Windows.Controls.Image image)
            {
                return;
            }

            if (e.NewValue is uint width)
            {
                OnChanged(image, GetSource(image), width, GetHeight(image));
            }
            else
            {
                OnChanged(image, GetSource(image), null, GetHeight(image));
            }
        }

        private static void OnHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not System.Windows.Controls.Image image)
            {
                return;
            }

            if (e.NewValue is uint height)
            {
                OnChanged(image, GetSource(image), GetWidth(image), height);
            }
            else
            {
                OnChanged(image, GetSource(image), GetWidth(image), null);
            }
        }

        private static async void OnChanged(System.Windows.Controls.Image sender, Uri? url, uint? width, uint? height)
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
