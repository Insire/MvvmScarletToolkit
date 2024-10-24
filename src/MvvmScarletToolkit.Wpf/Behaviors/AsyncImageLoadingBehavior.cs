using Microsoft.Xaml.Behaviors;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit
{
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:AsyncImageLoadingBehavior Source="{Binding Source}" Width="200" Height="200" />
    // </ i:Interaction.Behaviors>
    public sealed class AsyncImageLoadingBehavior : Behavior<Image>
    {
        public static Lazy<IImageService<BitmapSource>>? Loader { get; set; }

        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isEnabled;

        // size changed event subscription
        // debounce´size changed events
        // loading animation

        protected override void OnAttached()
        {
            base.OnAttached();

            _isEnabled = true;

            OnChanged(AssociatedObject, Source, Width, Height);
        }

        protected override void OnDetaching()
        {
            _isEnabled = false;

            base.OnDetaching();
        }

        public Uri? Source
        {
            get => (Uri?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnSourceChanged));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        public int? Width
        {
            get => (int?)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width),
            typeof(int),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null));

        public int? Height
        {
            get => (int?)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            "Height",
            typeof(int?),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null));

        private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not AsyncImageLoadingBehavior behavior)
            {
                return;
            }

            behavior.OnSourceChanged(e);
        }

        private void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Uri uri)
            {
                OnChanged(AssociatedObject, uri, Width, Height);
            }
            else
            {
                OnChanged(AssociatedObject, null, Width, Height);
            }
        }

        private static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not AsyncImageLoadingBehavior behavior)
            {
                return;
            }

            behavior.OnWidthChanged(e);
        }

        private void OnWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int width)
            {
                OnChanged(AssociatedObject, Source, width, Height);
            }
            else
            {
                OnChanged(AssociatedObject, Source, null, Height);
            }
        }

        private static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not AsyncImageLoadingBehavior behavior)
            {
                return;
            }

            behavior.OnHeightChanged(e);
        }

        private void OnHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int height)
            {
                OnChanged(AssociatedObject, Source, Width, height);
            }
            else
            {
                OnChanged(AssociatedObject, Source, Width, null);
            }
        }

        private async void OnChanged(Image sender, Uri? url, int? width, int? height)
        {
            if (Loader is null || !_isEnabled)
            {
                return;
            }

            var previousCts = _cancellationTokenSource;
            var currentCts = _cancellationTokenSource = new CancellationTokenSource();
            if (previousCts is not null)
            {
                previousCts.Cancel();
            }

            var size = GetSize(width, height);

            try
            {
                sender.Source = await Loader.Value
                    .ProvideImageAsync(url, size, SetIsLoading, currentCts.Token)
                    .ConfigureAwait(true);

                SetCurrentValue(IsLoadingProperty, false);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                currentCts.Dispose();
            }
        }

        private void SetIsLoading(bool isloading)
        {
            AssociatedObject.Dispatcher.Invoke(() => SetCurrentValue(IsLoadingProperty, isloading));
        }

        private static ImageSize? GetSize(int? width, int? height)
        {
            if (height is not null && width is not null)
            {
                return new ImageSize(width.Value, height.Value);
            }

            return null;
        }
    }
}
