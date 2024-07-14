using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    // original idea and code from: https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia
    public static class ImageLoader
    {
        public static RecyclableMemoryStreamManager Manager { get; set; } = default!;
        public static IImageFactory<BitmapSource> ImageFactory { get; set; } = default!;
        public static Lazy<IAsyncImageLoader<BitmapSource>> AsyncImageLoader { get; set; } = default!;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(Uri),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnSourceChanged));

        /// <summary>Helper for getting <see cref="SourceProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="SourceProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static Uri? GetSource(Image image)
        {
            return (Uri?)image.GetValue(SourceProperty);
        }

        /// <summary>Helper for setting <see cref="FirstProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="FrameworkElement"/> to set <see cref="SourceProperty"/> on.</param>
        /// <param name="value">First property value.</param>
        public static void SetSource(Image image, Uri? value)
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
        public static bool GetIsLoading(Image image)
        {
            return (bool)image.GetValue(IsLoadingProperty);
        }

        /// <summary>Helper for setting <see cref="IsLoadingProperty"/> on <paramref name="frameworkElement"/>.</summary>
        /// <param name="frameworkElement"><see cref="Image"/> to set <see cref="IsLoadingProperty"/> on.</param>
        /// <param name="value">First property value.</param>
        public static void SetIsLoading(Image frameworkElement, bool value)
        {
            frameworkElement.SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width",
            typeof(int?),
            typeof(ImageLoader),
            new PropertyMetadata(null));

        /// <summary>Helper for setting <see cref="WidthProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to set <see cref="WidthProperty"/> on.</param>
        public static int? GetWidth(Image image)
        {
            return (int?)image.GetValue(WidthProperty);
        }

        /// <summary>Helper for getting <see cref="WidthProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="WidthProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static void SetWidth(Image image, int? value)
        {
            image.SetValue(WidthProperty, value);
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached(
            "Height",
            typeof(int?),
            typeof(ImageLoader),
            new PropertyMetadata(null));

        /// <summary>Helper for setting <see cref="HeightProperty"/> on <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="FrameworkElement"/> to set <see cref="HeightProperty"/> on.</param>
        public static int? GetHeight(Image image)
        {
            return (int?)image.GetValue(HeightProperty);
        }

        /// <summary>Helper for getting <see cref="HeightProperty"/> from <paramref name="image"/>.</summary>
        /// <param name="image"><see cref="Image"/> to read <see cref="HeightProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static void SetHeight(Image image, int? value)
        {
            image.SetValue(HeightProperty, value);
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Image image)
            {
                if (e.NewValue is Uri uri)
                {
                    OnSourceChanged(image, uri);
                }
                else
                {
                    OnSourceChanged(image, null);
                }
            }
        }

        private static async void OnSourceChanged(Image sender, Uri? url)
        {
            if (GetSource(sender) != url)
            {
                return;
            }

            var height = GetHeight(sender);
            var width = GetWidth(sender);
            var size = GetSize(width, height);

            sender.Source = await AsyncImageLoader.Value.ProvideImageAsync(url, size, (isloading) => sender.Dispatcher.Invoke(() => SetIsLoading(sender, isloading)));

            SetIsLoading(sender, false);
        }

        private static ImageSize? GetSize(int? width, int? height)
        {
            if (height is not null && width is not null)
            {
                return new ImageSize(width.Value, height.Value);
            }
            else
            {
                if (height is null)
                {
                    if (width is null)
                    {
                        return null;
                    }

                    return new ImageSize(width.Value, 300);
                }

                if (width is null)
                {
                    if (height is null)
                    {
                        return null;
                    }

                    return new ImageSize(300, height.Value);
                }
            }

            return null;
        }
    }
}
