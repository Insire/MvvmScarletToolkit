using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    // original idea and code from: https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia
    public static class ImageLoader
    {
        private static readonly ConditionalWeakTable<Image, AsyncImageLoad> _loads = new();

        public static Lazy<IImageService<BitmapSource>>? AsyncImageLoader { get; set; }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(Uri),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnPropertyChanged));

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

        // State, IsPending, IsLoading, IsLoaded and IsFailed are set by the loader,
        // bind them with Mode=OneWayToSource to observe them from a view model.

        public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State",
            typeof(ImageLoadState),
            typeof(ImageLoader),
            new PropertyMetadata(ImageLoadState.None));

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static ImageLoadState GetState(FrameworkElement image) => (ImageLoadState)image.GetValue(StateProperty);

        public static void SetState(FrameworkElement image, ImageLoadState value) => image.SetValue(StateProperty, value);

        public static readonly DependencyProperty IsPendingProperty = DependencyProperty.RegisterAttached(
            "IsPending",
            typeof(bool),
            typeof(ImageLoader),
            new PropertyMetadata(false));

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetIsPending(FrameworkElement image) => (bool)image.GetValue(IsPendingProperty);

        public static void SetIsPending(FrameworkElement image, bool value) => image.SetValue(IsPendingProperty, value);

        public static readonly DependencyProperty IsLoadedProperty = DependencyProperty.RegisterAttached(
            "IsLoaded",
            typeof(bool),
            typeof(ImageLoader),
            new PropertyMetadata(false));

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetIsLoaded(FrameworkElement image) => (bool)image.GetValue(IsLoadedProperty);

        public static void SetIsLoaded(FrameworkElement image, bool value) => image.SetValue(IsLoadedProperty, value);

        public static readonly DependencyProperty IsFailedProperty = DependencyProperty.RegisterAttached(
            "IsFailed",
            typeof(bool),
            typeof(ImageLoader),
            new PropertyMetadata(false));

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetIsFailed(FrameworkElement image) => (bool)image.GetValue(IsFailedProperty);

        public static void SetIsFailed(FrameworkElement image, bool value) => image.SetValue(IsFailedProperty, value);

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width",
            typeof(uint?),
            typeof(ImageLoader),
            new PropertyMetadata(null, OnPropertyChanged));

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
            new PropertyMetadata(null, OnPropertyChanged));

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

        private static void OnPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not Image image)
            {
                return;
            }

            _loads.GetValue(image, CreateLoad).Invalidate();
        }

        private static AsyncImageLoad CreateLoad(Image image)
        {
            return new AsyncImageLoad(
                image,
                () => AsyncImageLoader,
                () => GetSource(image),
                () => AsyncImageLoad.GetSize(GetWidth(image), GetHeight(image)),
                state =>
                {
                    image.SetCurrentValue(StateProperty, state);
                    image.SetCurrentValue(IsPendingProperty, state == ImageLoadState.Pending);
                    image.SetCurrentValue(IsLoadingProperty, state == ImageLoadState.Loading);
                    image.SetCurrentValue(IsLoadedProperty, state == ImageLoadState.Loaded);
                    image.SetCurrentValue(IsFailedProperty, state == ImageLoadState.Failed);
                });
        }
    }
}
