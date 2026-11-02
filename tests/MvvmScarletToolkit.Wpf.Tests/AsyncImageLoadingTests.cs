using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    /// <summary>
    /// Runs the same scenarios against every way of loading images asynchronously, they all share their implementation.
    /// </summary>
    public abstract class AsyncImageLoadingTestsBase : TraceTestBase, IDisposable
    {
        protected static readonly Uri First = new("https://example.com/first.png");
        private static readonly Uri Second = new("https://example.com/second.png");

        protected readonly IImageService<BitmapSource> _imageService = Substitute.For<IImageService<BitmapSource>>();
        protected readonly BitmapSource _firstBitmap = TestBitmaps.Create(10, 10);
        private readonly BitmapSource _secondBitmap = TestBitmaps.Create(20, 20);

        protected AsyncImageLoadingTestsBase()
        {
            SetLoader(new Lazy<IImageService<BitmapSource>>(() => _imageService));

            Returns(First, _firstBitmap);
            Returns(Second, _secondBitmap);
        }

        public void Dispose()
        {
            SetLoader(null);
        }

        protected abstract void SetLoader(Lazy<IImageService<BitmapSource>>? loader);

        /// <summary>Creates an image that is set up for async loading, but not shown yet.</summary>
        protected abstract Image CreateImage();

        protected abstract void SetSource(Image image, Uri? source);

        protected abstract void SetWidth(Image image, uint? width);

        protected abstract void SetHeight(Image image, uint? height);

        /// <summary>The object that carries the state properties for <paramref name="image"/>.</summary>
        protected abstract DependencyObject GetStateTarget(Image image);

        protected abstract (DependencyProperty State, DependencyProperty IsPending, DependencyProperty IsLoading, DependencyProperty IsLoaded, DependencyProperty IsFailed) StateProperties { get; }

        private ImageLoadState GetState(Image image) => (ImageLoadState)GetStateTarget(image).GetValue(StateProperties.State);

        private bool GetIsLoading(Image image) => (bool)GetStateTarget(image).GetValue(StateProperties.IsLoading);

        /// <summary>Checks the state and that every boolean property agrees with it.</summary>
        private void AssertState(Image image, ImageLoadState expected)
        {
            var target = GetStateTarget(image);
            var properties = StateProperties;

            Assert.Equal(expected, (ImageLoadState)target.GetValue(properties.State));
            Assert.Equal(expected == ImageLoadState.Pending, (bool)target.GetValue(properties.IsPending));
            Assert.Equal(expected == ImageLoadState.Loading, (bool)target.GetValue(properties.IsLoading));
            Assert.Equal(expected == ImageLoadState.Loaded, (bool)target.GetValue(properties.IsLoaded));
            Assert.Equal(expected == ImageLoadState.Failed, (bool)target.GetValue(properties.IsFailed));
        }

        private sealed class StateViewModel
        {
            public ImageLoadState State { get; set; }

            public bool IsLoaded { get; set; }

            public bool IsFailed { get; set; }
        }

        private void Returns(Uri uri, BitmapSource bitmap)
        {
            _imageService.ProvideImageAsync(uri, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<BitmapSource?>(bitmap));
        }

        /// <summary>
        /// Returns a result for <paramref name="uri"/> that completes when the returned source is completed,
        /// or throws <see cref="OperationCanceledException"/> when cancelled, like the real image service.
        /// </summary>
        private TaskCompletionSource<BitmapSource?> ReturnsLater(Uri uri, TaskCompletionSource started)
        {
            var result = new TaskCompletionSource<BitmapSource?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _imageService.ProvideImageAsync(uri, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(async call =>
                {
                    await call.Arg<Func<bool, Task>>()(true);
                    started.TrySetResult();

                    var token = call.Arg<CancellationToken>();
                    await Task.WhenAny(result.Task, Task.Delay(Timeout.Infinite, token));
                    token.ThrowIfCancellationRequested();

                    return await result.Task;
                });

            return result;
        }

        protected static Window Show(Image image)
        {
            var window = new Window
            {
                Content = image,
                Width = 100,
                Height = 100,
                Left = -10000,
                Top = -10000,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
            };
            window.Show();

            return window;
        }

        protected (Window Window, Image Image) CreateShownImage()
        {
            var image = CreateImage();

            return (Show(image), image);
        }

        protected static async Task WaitUntil(Func<bool> condition)
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (!condition())
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new TimeoutException();
                }

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5);
            }
        }

        protected static async Task Settle()
        {
            for (var i = 0; i < 10; i++)
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5);
            }
        }

        [Fact]
        public Task Should_Load_Image_With_Requested_Size()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();

                SetWidth(image, 100);
                SetHeight(image, 50);
                SetSource(image, First);

                await WaitUntil(() => image.Source is not null);

                Assert.Same(_firstBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                await _imageService.Received(1).ProvideImageAsync(First, new ImageSize(100, 50), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Fact]
        public Task State_Should_Be_None_Without_Source()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();
                await Settle();

                AssertState(image, ImageLoadState.None);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Fail_When_Service_Returns_No_Image()
        {
            return StaRunner.Run(async () =>
            {
                // that is how the image service reports a 404, an undecodable image and similar
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<BitmapSource?>(null));
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => GetState(image) == ImageLoadState.Failed);

                Assert.Null(image.Source);
                AssertState(image, ImageLoadState.Failed);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Fail_When_No_Image_Service_Is_Configured()
        {
            return StaRunner.Run(async () =>
            {
                SetLoader(null);
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => GetState(image) == ImageLoadState.Failed);

                AssertState(image, ImageLoadState.Failed);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Recover_From_Failure_With_New_Source()
        {
            return StaRunner.Run(async () =>
            {
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<BitmapSource?>(null));
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => GetState(image) == ImageLoadState.Failed);

                SetSource(image, Second);
                await WaitUntil(() => GetState(image) == ImageLoadState.Loaded);

                Assert.Same(_secondBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task State_Should_Flow_Into_OneWayToSource_Bindings()
        {
            return StaRunner.Run(async () =>
            {
                _imageService.ProvideImageAsync(Second, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<BitmapSource?>(null));
                var viewModel = new StateViewModel();
                var image = CreateImage();
                var target = GetStateTarget(image);
                BindingOperations.SetBinding(target, StateProperties.State, new Binding(nameof(StateViewModel.State)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                BindingOperations.SetBinding(target, StateProperties.IsLoaded, new Binding(nameof(StateViewModel.IsLoaded)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                BindingOperations.SetBinding(target, StateProperties.IsFailed, new Binding(nameof(StateViewModel.IsFailed)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                var window = Show(image);

                SetSource(image, First);
                await WaitUntil(() => viewModel.State == ImageLoadState.Loaded);
                Assert.True(viewModel.IsLoaded);
                Assert.False(viewModel.IsFailed);

                SetSource(image, Second);
                await WaitUntil(() => viewModel.State == ImageLoadState.Failed);
                Assert.False(viewModel.IsLoaded);
                Assert.True(viewModel.IsFailed);

                // the bindings must survive the loader updating the values
                Assert.NotNull(BindingOperations.GetBindingExpression(target, StateProperties.State));
                window.Close();
            });
        }

        [Fact]
        public Task Should_Request_Default_Size_When_Size_Is_Incomplete()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();

                SetWidth(image, 100);
                SetSource(image, First);

                await WaitUntil(() => image.Source is not null);

                await _imageService.Received(1).ProvideImageAsync(First, null, Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Fact]
        public Task Property_Changes_In_A_Row_Should_Result_In_A_Single_Load()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                SetWidth(image, 100);
                SetHeight(image, 100);

                await WaitUntil(() => image.Source is not null);
                await Settle();

                Assert.Single(_imageService.ReceivedCalls());
                window.Close();
            });
        }

        [Fact]
        public Task Should_Report_IsLoading_While_Loading_Slowly()
        {
            return StaRunner.Run(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var pending = ReturnsLater(First, started);
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);
                await WaitUntil(() => GetIsLoading(image));
                AssertState(image, ImageLoadState.Loading);

                pending.SetResult(_firstBitmap);

                await WaitUntil(() => image.Source is not null);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Superseded_Load_Should_Not_Overwrite_Newer_Image()
        {
            return StaRunner.Run(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var first = new TaskCompletionSource<BitmapSource?>(TaskCreationOptions.RunContinuationsAsynchronously);

                // ignores cancellation and completes late, like a service that does not honor the token
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(_ =>
                    {
                        started.TrySetResult();
                        return first.Task;
                    });

                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                SetSource(image, Second);
                await WaitUntil(() => image.Source is not null);

                first.SetResult(_firstBitmap);
                await Settle();

                Assert.Same(_secondBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Superseded_Load_Cancelled_With_OperationCanceledException_Should_Not_Crash()
        {
            // the image service throws OperationCanceledException (not only TaskCanceledException) on cancellation,
            // escaping it from the async void handler would crash the application
            return StaRunner.Run(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                ReturnsLater(First, started);
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                SetSource(image, Second);

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_secondBitmap, image.Source);
                await WaitUntil(() => !GetIsLoading(image));
                window.Close();
            });
        }

        [Fact]
        public Task Failing_Load_Should_Not_Crash()
        {
            return StaRunner.Run(async () =>
            {
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromException<BitmapSource?>(new InvalidOperationException("boom")));
                var (window, image) = CreateShownImage();

                SetSource(image, First);
                await WaitUntil(() => _imageService.ReceivedCalls().Any());
                await Settle();

                Assert.Null(image.Source);
                AssertState(image, ImageLoadState.Failed);
                window.Close();
            });
        }

        [Fact]
        public Task Setting_Source_To_Null_Should_Clear_Image()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => image.Source is not null);

                SetSource(image, null);

                await WaitUntil(() => image.Source is null);
                Assert.Single(_imageService.ReceivedCalls());
                AssertState(image, ImageLoadState.None);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Not_Load_Until_Loaded()
        {
            return StaRunner.Run(async () =>
            {
                var image = CreateImage();
                SetSource(image, First);
                await Settle();

                Assert.Empty(_imageService.ReceivedCalls());
                AssertState(image, ImageLoadState.Pending);

                var window = Show(image);

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_firstBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Unloading_Should_Cancel_And_Loading_Again_Should_Resume()
        {
            return StaRunner.Run(async () =>
            {
                CancellationToken firstToken = default;
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(async call =>
                    {
                        var token = call.Arg<CancellationToken>();
                        if (!started.Task.IsCompleted)
                        {
                            firstToken = token;
                            started.SetResult();
                            await Task.Delay(Timeout.Infinite, token).ContinueWith(_ => { }, TaskScheduler.Default);
                            token.ThrowIfCancellationRequested();
                        }

                        return (BitmapSource?)_firstBitmap;
                    });

                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                window.Content = null;
                await WaitUntil(() => firstToken.IsCancellationRequested);
                await Settle();
                Assert.Null(image.Source);
                AssertState(image, ImageLoadState.Pending);

                window.Content = image;

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_firstBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                Assert.Equal(2, _imageService.ReceivedCalls().Count());
                window.Close();
            });
        }

        [Fact]
        public Task Cancellation_Reported_As_Null_Should_Stay_Pending_And_Resume()
        {
            // the image service reports cancellation with null instead of an exception
            return StaRunner.Run(async () =>
            {
                CancellationToken firstToken = default;
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(async call =>
                    {
                        var token = call.Arg<CancellationToken>();
                        if (!started.Task.IsCompleted)
                        {
                            firstToken = token;
                            started.SetResult();
                            await Task.Delay(Timeout.Infinite, token).ContinueWith(_ => { }, TaskScheduler.Default);

                            return null;
                        }

                        return (BitmapSource?)_firstBitmap;
                    });

                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                window.Content = null;
                await WaitUntil(() => firstToken.IsCancellationRequested);
                await Settle();

                // not a failure, the image was just not needed anymore
                AssertState(image, ImageLoadState.Pending);

                window.Content = image;

                await WaitUntil(() => image.Source is not null);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Loading_Again_Before_The_Cancelled_Load_Finished_Should_Resume()
        {
            // e.g. container recycling in virtualized lists, Loaded arrives while the cancelled load is still in flight
            return StaRunner.Run(async () =>
            {
                var calls = 0;
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(async call =>
                    {
                        if (Interlocked.Increment(ref calls) == 1)
                        {
                            started.SetResult();

                            // only notices the cancellation after being loaded again
                            await release.Task;
                            call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                        }

                        return (BitmapSource?)_firstBitmap;
                    });

                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                window.Content = null;
                await Settle();
                window.Content = image;
                await Settle();

                release.SetResult();

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_firstBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                Assert.Equal(2, calls);
                window.Close();
            });
        }

        [Fact]
        public Task Loading_A_Loaded_Image_Again_Should_Not_Load_Again()
        {
            return StaRunner.Run(async () =>
            {
                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => image.Source is not null);

                window.Content = null;
                await Settle();
                window.Content = image;
                await Settle();

                // the previous implementation cleared the image on unload and never reloaded it
                Assert.Same(_firstBitmap, image.Source);
                Assert.Single(_imageService.ReceivedCalls());
                window.Close();
            });
        }
    }

    // AsyncImageLoadingBehavior.Loader is static, tests in this class must not run in parallel with other users of it
    public sealed class AsyncImageLoadingBehaviorTests : AsyncImageLoadingTestsBase
    {
        protected override void SetLoader(Lazy<IImageService<BitmapSource>>? loader) => AsyncImageLoadingBehavior.Loader = loader;

        protected override Image CreateImage()
        {
            var image = new Image();
            Interaction.GetBehaviors(image).Add(new AsyncImageLoadingBehavior());

            return image;
        }

        private static AsyncImageLoadingBehavior GetBehavior(Image image) => Interaction.GetBehaviors(image).OfType<AsyncImageLoadingBehavior>().Single();

        protected override void SetSource(Image image, Uri? source) => GetBehavior(image).Source = source;

        protected override void SetWidth(Image image, uint? width) => GetBehavior(image).Width = width;

        protected override void SetHeight(Image image, uint? height) => GetBehavior(image).Height = height;

        protected override DependencyObject GetStateTarget(Image image) => GetBehavior(image);

        protected override (DependencyProperty State, DependencyProperty IsPending, DependencyProperty IsLoading, DependencyProperty IsLoaded, DependencyProperty IsFailed) StateProperties => (
            AsyncImageLoadingBehavior.StateProperty,
            AsyncImageLoadingBehavior.IsPendingProperty,
            AsyncImageLoadingBehavior.IsLoadingProperty,
            AsyncImageLoadingBehavior.IsLoadedProperty,
            AsyncImageLoadingBehavior.IsFailedProperty);

        [Fact]
        public Task Detaching_Behavior_Should_Drop_In_Flight_Result()
        {
            return StaRunner.Run(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var first = new TaskCompletionSource<BitmapSource?>(TaskCreationOptions.RunContinuationsAsynchronously);
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(_ =>
                    {
                        started.TrySetResult();
                        return first.Task;
                    });

                var (window, image) = CreateShownImage();
                SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                Interaction.GetBehaviors(image).Remove(GetBehavior(image));
                first.SetResult(_firstBitmap);
                await Settle();

                Assert.Null(image.Source);
                window.Close();
            });
        }
    }

    // ImageLoader.AsyncImageLoader is static, tests in this class must not run in parallel with other users of it
    public sealed class ImageLoaderTests : AsyncImageLoadingTestsBase
    {
        protected override void SetLoader(Lazy<IImageService<BitmapSource>>? loader) => ImageLoader.AsyncImageLoader = loader;

        protected override Image CreateImage() => new();

        protected override void SetSource(Image image, Uri? source) => ImageLoader.SetSource(image, source);

        protected override void SetWidth(Image image, uint? width) => ImageLoader.SetWidth(image, width);

        protected override void SetHeight(Image image, uint? height) => ImageLoader.SetHeight(image, height);

        protected override DependencyObject GetStateTarget(Image image) => image;

        protected override (DependencyProperty State, DependencyProperty IsPending, DependencyProperty IsLoading, DependencyProperty IsLoaded, DependencyProperty IsFailed) StateProperties => (
            ImageLoader.StateProperty,
            ImageLoader.IsPendingProperty,
            ImageLoader.IsLoadingProperty,
            ImageLoader.IsLoadedProperty,
            ImageLoader.IsFailedProperty);
    }
}
