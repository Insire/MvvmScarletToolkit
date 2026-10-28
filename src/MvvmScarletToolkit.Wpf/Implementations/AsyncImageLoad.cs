using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Loads the image source of an <see cref="Image"/> asynchronously. Shared by <see cref="AsyncImageLoadingBehavior"/> and <see cref="ImageLoader"/>.
    /// <list type="bullet">
    /// <item>Several property changes in a row result in a single load.</item>
    /// <item>A superseded load never overwrites the result of a newer one.</item>
    /// <item>Loads only start while the image is loaded, unloading cancels them, loading again resumes them.</item>
    /// <item>Exceptions never escape, since loading is triggered from async void code.</item>
    /// </list>
    /// </summary>
    internal sealed class AsyncImageLoad
    {
        private readonly Image _image;
        private readonly Func<Lazy<IImageService<BitmapSource>>?> _getLoader;
        private readonly Func<Uri?> _getSource;
        private readonly Func<ImageSize?> _getSize;
        private readonly Action<ImageLoadState> _setState;

        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>Incremented on every change, a load only applies its result while it is still the latest one.</summary>
        private int _version;

        private bool _isScheduled;

        public AsyncImageLoad(
            Image image,
            Func<Lazy<IImageService<BitmapSource>>?> getLoader,
            Func<Uri?> getSource,
            Func<ImageSize?> getSize,
            Action<ImageLoadState> setState)
        {
            _image = image;
            _getLoader = getLoader;
            _getSource = getSource;
            _getSize = getSize;
            _setState = setState;

            _image.Loaded += OnLoaded;
            _image.Unloaded += OnUnloaded;
        }

        public ImageLoadState State { get; private set; }

        /// <summary>
        /// Stops loading and drops whatever is still in flight.
        /// </summary>
        public void Detach()
        {
            _image.Loaded -= OnLoaded;
            _image.Unloaded -= OnUnloaded;

            _version++;
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Cancels the current load and schedules a new one.
        /// </summary>
        public void Invalidate()
        {
            _version++;
            _cancellationTokenSource?.Cancel();
            SetState(ImageLoadState.Pending);

            if (_isScheduled)
            {
                return;
            }
            _isScheduled = true;
            _image.Dispatcher.BeginInvoke(DispatcherPriority.Background, Load);
        }

        private void SetState(ImageLoadState state)
        {
            State = state;
            _setState(state);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (State == ImageLoadState.Pending && _cancellationTokenSource is null)
            {
                Invalidate();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // stop wasting bandwidth on images that are not shown anymore, e.g. in virtualized lists
            _cancellationTokenSource?.Cancel();
        }

        private async void Load()
        {
            _isScheduled = false;

            var version = _version;
            var uri = _getSource();

            if (uri is null)
            {
                _image.Source = null;
                SetState(ImageLoadState.None);

                return;
            }

            var loader = _getLoader();
            if (loader is null)
            {
                Trace.TraceWarning("No image service has been configured to load {0}", uri);
                _image.Source = null;
                SetState(ImageLoadState.Failed);

                return;
            }

            if (!_image.IsLoaded)
            {
                // not shown, stays pending until OnLoaded picks it up
                return;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;

            try
            {
                var bitmap = await loader.Value.ProvideImageAsync(uri, _getSize(), isLoading => SetIsLoadingAsync(version, isLoading), cancellationTokenSource.Token);

                if (_version != version)
                {
                    // superseded
                }
                else if (bitmap is null && cancellationTokenSource.IsCancellationRequested)
                {
                    // unloaded, the image service reports cancellation with null, resumes once loaded again
                    SetState(ImageLoadState.Pending);
                }
                else
                {
                    // otherwise null means anything went wrong, e.g. a 404
                    _image.Source = bitmap;
                    SetState(bitmap is null ? ImageLoadState.Failed : ImageLoadState.Loaded);
                }
            }
            catch (OperationCanceledException)
            {
                // superseded or unloaded, the latter resumes once loaded again
                if (_version == version)
                {
                    SetState(ImageLoadState.Pending);
                }
            }
            catch (Exception ex)
            {
                // async void, an escaping exception would crash the application
                Trace.TraceWarning("Loading image from {0} failed: {1}", uri, ex);

                if (_version == version)
                {
                    SetState(ImageLoadState.Failed);
                }
            }
            finally
            {
                if (ReferenceEquals(_cancellationTokenSource, cancellationTokenSource))
                {
                    _cancellationTokenSource = null;
                }

                cancellationTokenSource.Dispose();
            }
        }

        private Task SetIsLoadingAsync(int version, bool isLoading)
        {
            // the image service may call this from any thread. Not awaited on purpose,
            // the image service would otherwise wait for the UI thread before it continues loading
            _image.Dispatcher.InvokeAsync(() =>
            {
                if (_version == version && State is ImageLoadState.Pending or ImageLoadState.Loading)
                {
                    SetState(isLoading ? ImageLoadState.Loading : ImageLoadState.Pending);
                }
            });

            return Task.CompletedTask;
        }

        public static ImageSize? GetSize(uint? width, uint? height)
        {
            if (height is not null && width is not null)
            {
                return new ImageSize(width.Value, height.Value);
            }

            return null;
        }
    }
}
