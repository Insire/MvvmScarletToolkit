using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit
{
    // usage:
    // <Image mvvm:AsyncImageLoadingBehavior.Source="{Binding Source}"
    //        mvvm:AsyncImageLoadingBehavior.Width="200"
    //        mvvm:AsyncImageLoadingBehavior.Height="200" />
    //
    // State, IsPending, IsLoading, IsLoaded and IsFailed are set by the behavior,
    // bind them with Mode=OneWayToSource to observe them from a view model.
    public sealed class AsyncImageLoadingBehavior
    {
        private sealed class LoadState
        {
            /// <summary>Incremented on every change, a load only applies its result while it is still the latest one.</summary>
            public int Version { get; set; }

            public CancellationTokenSource? CancellationTokenSource { get; set; }

            public bool IsScheduled { get; set; }

            public ImageLoadState State { get; set; }
        }

        private static readonly ConditionalWeakTable<Image, LoadState> _states = new();

        public static Lazy<IImageService<Bitmap>>? Loader { get; set; }

        public static readonly AttachedProperty<Uri?> SourceProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, Uri?>("Source");

        public static readonly AttachedProperty<uint?> WidthProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, uint?>("Width");

        public static readonly AttachedProperty<uint?> HeightProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, uint?>("Height");

        public static readonly AttachedProperty<ImageLoadState> StateProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, ImageLoadState>("State");

        public static readonly AttachedProperty<bool> IsPendingProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, bool>("IsPending");

        public static readonly AttachedProperty<bool> IsLoadingProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, bool>("IsLoading");

        public static readonly AttachedProperty<bool> IsLoadedProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, bool>("IsLoaded");

        public static readonly AttachedProperty<bool> IsFailedProperty = AvaloniaProperty.RegisterAttached<AsyncImageLoadingBehavior, Image, bool>("IsFailed");

        static AsyncImageLoadingBehavior()
        {
            SourceProperty.Changed.AddClassHandler<Image, Uri?>((image, _) => Invalidate(image));
            WidthProperty.Changed.AddClassHandler<Image, uint?>((image, _) => Invalidate(image));
            HeightProperty.Changed.AddClassHandler<Image, uint?>((image, _) => Invalidate(image));
        }

        private AsyncImageLoadingBehavior()
        {
        }

        public static Uri? GetSource(Image image) => image.GetValue(SourceProperty);

        public static void SetSource(Image image, Uri? value) => image.SetValue(SourceProperty, value);

        public static uint? GetWidth(Image image) => image.GetValue(WidthProperty);

        public static void SetWidth(Image image, uint? value) => image.SetValue(WidthProperty, value);

        public static uint? GetHeight(Image image) => image.GetValue(HeightProperty);

        public static void SetHeight(Image image, uint? value) => image.SetValue(HeightProperty, value);

        public static ImageLoadState GetState(Image image) => image.GetValue(StateProperty);

        public static void SetState(Image image, ImageLoadState value) => image.SetValue(StateProperty, value);

        public static bool GetIsPending(Image image) => image.GetValue(IsPendingProperty);

        public static void SetIsPending(Image image, bool value) => image.SetValue(IsPendingProperty, value);

        public static bool GetIsLoading(Image image) => image.GetValue(IsLoadingProperty);

        public static void SetIsLoading(Image image, bool value) => image.SetValue(IsLoadingProperty, value);

        public static bool GetIsLoaded(Image image) => image.GetValue(IsLoadedProperty);

        public static void SetIsLoaded(Image image, bool value) => image.SetValue(IsLoadedProperty, value);

        public static bool GetIsFailed(Image image) => image.GetValue(IsFailedProperty);

        public static void SetIsFailed(Image image, bool value) => image.SetValue(IsFailedProperty, value);

        private static LoadState GetLoadState(Image image)
        {
            if (_states.TryGetValue(image, out var state))
            {
                return state;
            }

            state = new LoadState();
            _states.Add(image, state);

            image.AttachedToVisualTree += OnAttachedToVisualTree;
            image.DetachedFromVisualTree += OnDetachedFromVisualTree;

            return state;
        }

        private static void UpdateState(Image image, LoadState state, ImageLoadState value)
        {
            state.State = value;

            // SetCurrentValue, so bindings or styles targeting these properties are not replaced
            image.SetCurrentValue(StateProperty, value);
            image.SetCurrentValue(IsPendingProperty, value == ImageLoadState.Pending);
            image.SetCurrentValue(IsLoadingProperty, value == ImageLoadState.Loading);
            image.SetCurrentValue(IsLoadedProperty, value == ImageLoadState.Loaded);
            image.SetCurrentValue(IsFailedProperty, value == ImageLoadState.Failed);
        }

        private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is Image image && _states.TryGetValue(image, out var state) && state.State == ImageLoadState.Pending && state.CancellationTokenSource is null)
            {
                Invalidate(image);
            }
        }

        private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // stop wasting bandwidth on images that are not shown anymore, e.g. in virtualized lists
            if (sender is Image image && _states.TryGetValue(image, out var state))
            {
                state.CancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Cancels the current load and schedules a new one. Several property changes in a row result in a single load.
        /// </summary>
        private static void Invalidate(Image image)
        {
            var state = GetLoadState(image);
            state.Version++;
            state.CancellationTokenSource?.Cancel();
            UpdateState(image, state, ImageLoadState.Pending);

            if (state.IsScheduled)
            {
                return;
            }

            state.IsScheduled = true;
            Dispatcher.UIThread.Post(() =>
            {
                state.IsScheduled = false;
                Load(image, state);
            }, DispatcherPriority.Background);
        }

        private static async void Load(Image image, LoadState state)
        {
            var version = state.Version;
            var uri = GetSource(image);

            if (uri is null)
            {
                image.Source = null;
                UpdateState(image, state, ImageLoadState.None);

                return;
            }

            var loader = Loader;
            if (loader is null)
            {
                Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(image, "No image service has been configured to load {Uri}", uri);
                image.Source = null;
                UpdateState(image, state, ImageLoadState.Failed);

                return;
            }

            if (!image.IsAttachedToVisualTree())
            {
                // not shown, stays pending until OnAttachedToVisualTree picks it up
                return;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            state.CancellationTokenSource = cancellationTokenSource;

            try
            {
                var bitmap = await loader.Value.ProvideImageAsync(uri, GetSize(image), isLoading => SetIsLoadingAsync(image, state, version, isLoading), cancellationTokenSource.Token);

                if (state.Version != version)
                {
                    // superseded
                }
                else if (bitmap is null && cancellationTokenSource.IsCancellationRequested)
                {
                    // detached, the image service reports cancellation with null, resumes once attached again
                    UpdateState(image, state, ImageLoadState.Pending);
                }
                else
                {
                    // otherwise null means anything went wrong, e.g. a 404
                    image.Source = bitmap;
                    UpdateState(image, state, bitmap is null ? ImageLoadState.Failed : ImageLoadState.Loaded);
                }
            }
            catch (OperationCanceledException)
            {
                // superseded or detached, the latter resumes once attached again
                if (state.Version == version)
                {
                    UpdateState(image, state, ImageLoadState.Pending);
                }
            }
            catch (Exception ex)
            {
                // async void, an escaping exception would crash the application
                Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(image, "Loading image from {Uri} failed: {Exception}", uri, ex);

                if (state.Version == version)
                {
                    UpdateState(image, state, ImageLoadState.Failed);
                }
            }
            finally
            {
                if (ReferenceEquals(state.CancellationTokenSource, cancellationTokenSource))
                {
                    state.CancellationTokenSource = null;
                }

                cancellationTokenSource.Dispose();
            }
        }

        private static Task SetIsLoadingAsync(Image image, LoadState state, int version, bool isLoading)
        {
            // the image service may call this from any thread. Not awaited on purpose,
            // the image service would otherwise wait for the UI thread before it continues loading
            Dispatcher.UIThread.Post(() =>
            {
                if (state.Version == version && state.State is ImageLoadState.Pending or ImageLoadState.Loading)
                {
                    UpdateState(image, state, isLoading ? ImageLoadState.Loading : ImageLoadState.Pending);
                }
            });

            return Task.CompletedTask;
        }

        private static ImageSize? GetSize(Image image)
        {
            var width = GetWidth(image);
            var height = GetHeight(image);

            return width is not null && height is not null
                ? new ImageSize(width.Value, height.Value)
                : null;
        }
    }
}
