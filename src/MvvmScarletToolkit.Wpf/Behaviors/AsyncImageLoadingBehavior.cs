using Microsoft.Xaml.Behaviors;
using System;
using System.Diagnostics;
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

            AssociatedObject.Initialized -= OnInitialized;
            AssociatedObject.Initialized += OnInitialized;
            AssociatedObject.Unloaded -= OnUnloaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        protected override void OnDetaching()
        {
            _isEnabled = false;

            AssociatedObject.Unloaded -= OnUnloaded;

            base.OnDetaching();
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            OnChanged("OnInitialized", AssociatedObject, null, Source, null, Width, null, Height);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            OnChanged("OnUnloaded", AssociatedObject, Source, null, Width, null, Height, null);
        }

        public Uri? Source
        {
            get { return (Uri?)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnSourceChanged));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        public uint? Width
        {
            get { return (uint?)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width),
            typeof(uint?),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnWidthChanged));

        public uint? Height
        {
            get { return (uint?)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            nameof(Height),
            typeof(uint?),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnHeightChanged));

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
                OnChanged("OnSourceChanged", AssociatedObject, e.OldValue as Uri, uri, Width, Width, Height, Height);
            }
            else
            {
                OnChanged("OnSourceChanged", AssociatedObject, e.OldValue as Uri, null, Width, Width, Height, Height);
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
            if (e.NewValue is uint width)
            {
                OnChanged("OnWidthChanged", AssociatedObject, Source, Source, e.OldValue as uint?, width, Height, Height);
            }
            else
            {
                OnChanged("OnWidthChanged", AssociatedObject, Source, Source, e.OldValue as uint?, null, Height, Height);
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
            if (e.NewValue is uint height)
            {
                OnChanged("OnHeightChanged", AssociatedObject, Source, Source, Width, Width, e.OldValue as uint?, height);
            }
            else
            {
                OnChanged("OnHeightChanged", AssociatedObject, Source, Source, Width, Width, e.OldValue as uint?, null);
            }
        }

        private async void OnChanged(string scope, Image sender, Uri? oldUrl, Uri? newUrl, uint? oldWidth, uint? newWidth, uint? oldHeight, uint? newHeight)
        {
            if (Loader is null || !_isEnabled)
            {
                return;
            }

            if (newUrl is null && sender.Source is null)
            {
                return;
            }

            if (!sender.IsInitialized)
            {
                return;
            }

            var currentCts = new CancellationTokenSource();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = currentCts;

            var size = GetSize(newWidth, newHeight);

            try
            {
                Debug.WriteLine($"OnChanged: scope:\"{scope}\", url:\"{oldUrl}\" -> \"{newUrl}\" width: \"{oldWidth}\"->\"{newWidth}\" height: \"{oldHeight}\"->\"{newHeight}\"");

                sender.Source = await Loader.Value
                    .ProvideImageAsync(newUrl, size, SetIsLoading, currentCts.Token);

                SetIsLoadingOnUiThread(false);
            }
            catch (TaskCanceledException)
            {
                SetCurrentValue(IsLoadingProperty, false);
                Debug.WriteLine($"Loading url:\"{newUrl}\" cancelled");
            }
            finally
            {
                _cancellationTokenSource = null;
                currentCts.Dispose();
            }
        }

        private async Task SetIsLoading(bool isloading)
        {
            await AssociatedObject.Dispatcher.BeginInvoke(() => SetIsLoadingOnUiThread(isloading));
        }

        private void SetIsLoadingOnUiThread(bool isloading)
        {
            SetCurrentValue(IsLoadingProperty, isloading);
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
