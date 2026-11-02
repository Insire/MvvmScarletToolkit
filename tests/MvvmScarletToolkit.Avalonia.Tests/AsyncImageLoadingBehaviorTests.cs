using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MvvmScarletToolkit.Avalonia.Tests.Util;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    // AsyncImageLoadingBehavior.Loader is static, tests in this class must not run in parallel with other users of it
    public sealed class AsyncImageLoadingBehaviorTests : TraceTestBase, IDisposable
    {
        private static readonly Uri First = new("https://example.com/first.png");
        private static readonly Uri Second = new("https://example.com/second.png");

        private readonly AvaloniaPlatformFixture _platform;
        private readonly IImageService<Bitmap> _imageService = Substitute.For<IImageService<Bitmap>>();
        private readonly Bitmap _firstBitmap = TestBitmaps.Create(10, 10);
        private readonly Bitmap _secondBitmap = TestBitmaps.Create(20, 20);

        // TestContext.Current does not flow to the Avalonia UI thread, capture what is needed up front
        private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

        public AsyncImageLoadingBehaviorTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
            AsyncImageLoadingBehavior.Loader = new Lazy<IImageService<Bitmap>>(() => _imageService);

            Returns(First, _firstBitmap);
            Returns(Second, _secondBitmap);
        }

        public void Dispose()
        {
            AsyncImageLoadingBehavior.Loader = null;
            _firstBitmap.Dispose();
            _secondBitmap.Dispose();
        }

        private void Returns(Uri uri, Bitmap bitmap)
        {
            _imageService.ProvideImageAsync(uri, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Bitmap?>(bitmap));
        }

        /// <summary>
        /// Returns a result for <paramref name="uri"/> that completes when the returned source is completed,
        /// or throws <see cref="OperationCanceledException"/> when cancelled, like the real image service.
        /// </summary>
        private TaskCompletionSource<Bitmap?> ReturnsLater(Uri uri, TaskCompletionSource? started = null)
        {
            var result = new TaskCompletionSource<Bitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _imageService.ProvideImageAsync(uri, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(async call =>
                {
                    await call.Arg<Func<bool, Task>>()(true);
                    started?.TrySetResult();

                    var token = call.Arg<CancellationToken>();
                    await Task.WhenAny(result.Task, Task.Delay(Timeout.Infinite, token));
                    token.ThrowIfCancellationRequested();

                    return await result.Task;
                });

            return result;
        }

        /// <summary>Checks the state and that every boolean property agrees with it.</summary>
        private static void AssertState(Image image, ImageLoadState expected)
        {
            Assert.Equal(expected, AsyncImageLoadingBehavior.GetState(image));
            Assert.Equal(expected == ImageLoadState.Pending, AsyncImageLoadingBehavior.GetIsPending(image));
            Assert.Equal(expected == ImageLoadState.Loading, AsyncImageLoadingBehavior.GetIsLoading(image));
            Assert.Equal(expected == ImageLoadState.Loaded, AsyncImageLoadingBehavior.GetIsLoaded(image));
            Assert.Equal(expected == ImageLoadState.Failed, AsyncImageLoadingBehavior.GetIsFailed(image));
        }

        private sealed class StateViewModel
        {
            public ImageLoadState State { get; set; }

            public bool IsLoaded { get; set; }

            public bool IsFailed { get; set; }
        }

        private static (Window Window, Image Image) CreateShownImage()
        {
            var image = new Image();
            var window = new Window { Content = image };
            window.Show();

            return (window, image);
        }

        private async Task WaitUntil(Func<bool> condition)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (!condition())
            {
                timeout.Token.ThrowIfCancellationRequested();
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(5, _cancellationToken);
            }
        }

        private async Task Settle()
        {
            for (var i = 0; i < 10; i++)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(5, _cancellationToken);
            }
        }

        [Fact]
        public Task Should_Load_Image_With_Requested_Size()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetWidth(image, 100);
                AsyncImageLoadingBehavior.SetHeight(image, 50);
                AsyncImageLoadingBehavior.SetSource(image, First);

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
            return _platform.RunOnUIThread(async () =>
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
            return _platform.RunOnUIThread(async () =>
            {
                // that is how the image service reports a 404, an undecodable image and similar
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Bitmap?>(null));
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => AsyncImageLoadingBehavior.GetState(image) == ImageLoadState.Failed);

                Assert.Null(image.Source);
                AssertState(image, ImageLoadState.Failed);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Fail_When_No_Image_Service_Is_Configured()
        {
            return _platform.RunOnUIThread(async () =>
            {
                AsyncImageLoadingBehavior.Loader = null;
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => AsyncImageLoadingBehavior.GetState(image) == ImageLoadState.Failed);

                AssertState(image, ImageLoadState.Failed);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Recover_From_Failure_With_New_Source()
        {
            return _platform.RunOnUIThread(async () =>
            {
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Bitmap?>(null));
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => AsyncImageLoadingBehavior.GetState(image) == ImageLoadState.Failed);

                AsyncImageLoadingBehavior.SetSource(image, Second);
                await WaitUntil(() => AsyncImageLoadingBehavior.GetState(image) == ImageLoadState.Loaded);

                Assert.Same(_secondBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task State_Should_Flow_Into_OneWayToSource_Bindings()
        {
            return _platform.RunOnUIThread(async () =>
            {
                _imageService.ProvideImageAsync(Second, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Bitmap?>(null));
                var viewModel = new StateViewModel();
                var image = new Image();
                image.Bind(AsyncImageLoadingBehavior.StateProperty, new Binding(nameof(StateViewModel.State)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                image.Bind(AsyncImageLoadingBehavior.IsLoadedProperty, new Binding(nameof(StateViewModel.IsLoaded)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                image.Bind(AsyncImageLoadingBehavior.IsFailedProperty, new Binding(nameof(StateViewModel.IsFailed)) { Source = viewModel, Mode = BindingMode.OneWayToSource });
                var window = new Window { Content = image };
                window.Show();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => viewModel.State == ImageLoadState.Loaded);
                Assert.True(viewModel.IsLoaded);
                Assert.False(viewModel.IsFailed);

                AsyncImageLoadingBehavior.SetSource(image, Second);
                await WaitUntil(() => viewModel.State == ImageLoadState.Failed);
                Assert.False(viewModel.IsLoaded);
                Assert.True(viewModel.IsFailed);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Request_Default_Size_When_Size_Is_Incomplete()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetWidth(image, 100);
                AsyncImageLoadingBehavior.SetSource(image, First);

                await WaitUntil(() => image.Source is not null);

                await _imageService.Received(1).ProvideImageAsync(First, null, Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Fact]
        public Task Property_Changes_In_A_Row_Should_Result_In_A_Single_Load()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                AsyncImageLoadingBehavior.SetWidth(image, 100);
                AsyncImageLoadingBehavior.SetHeight(image, 100);

                await WaitUntil(() => image.Source is not null);
                await Settle();

                Assert.Single(_imageService.ReceivedCalls());
                window.Close();
            });
        }

        [Fact]
        public Task Should_Report_IsLoading_While_Loading_Slowly()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var pending = ReturnsLater(First, started);
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);
                await WaitUntil(() => AsyncImageLoadingBehavior.GetIsLoading(image));
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
            return _platform.RunOnUIThread(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var first = new TaskCompletionSource<Bitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);

                // ignores cancellation and completes late, like a service that does not honor the token
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(_ =>
                    {
                        started.TrySetResult();
                        return first.Task;
                    });

                var (window, image) = CreateShownImage();
                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                AsyncImageLoadingBehavior.SetSource(image, Second);
                await WaitUntil(() => image.Source is not null);

                first.SetResult(_firstBitmap);
                await Settle();

                Assert.Same(_secondBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Cancelled_Load_Should_Not_Crash()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                ReturnsLater(First, started);
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => started.Task.IsCompleted);

                AsyncImageLoadingBehavior.SetSource(image, Second);

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_secondBitmap, image.Source);
                window.Close();
            });
        }

        [Fact]
        public Task Failing_Load_Should_Not_Crash()
        {
            return _platform.RunOnUIThread(async () =>
            {
                _imageService.ProvideImageAsync(First, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromException<Bitmap?>(new InvalidOperationException("boom")));
                var (window, image) = CreateShownImage();

                AsyncImageLoadingBehavior.SetSource(image, First);
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
            return _platform.RunOnUIThread(async () =>
            {
                var (window, image) = CreateShownImage();
                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => image.Source is not null);

                AsyncImageLoadingBehavior.SetSource(image, null);

                await WaitUntil(() => image.Source is null);
                Assert.Single(_imageService.ReceivedCalls());
                AssertState(image, ImageLoadState.None);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Not_Load_Until_Attached_To_Visual_Tree()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var image = new Image();
                AsyncImageLoadingBehavior.SetSource(image, First);
                await Settle();

                Assert.Empty(_imageService.ReceivedCalls());
                AssertState(image, ImageLoadState.Pending);

                var window = new Window { Content = image };
                window.Show();

                await WaitUntil(() => image.Source is not null);
                Assert.Same(_firstBitmap, image.Source);
                AssertState(image, ImageLoadState.Loaded);
                window.Close();
            });
        }

        [Fact]
        public Task Detaching_Should_Cancel_And_Reattaching_Should_Resume()
        {
            return _platform.RunOnUIThread(async () =>
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

                        return (Bitmap?)_firstBitmap;
                    });

                var (window, image) = CreateShownImage();
                AsyncImageLoadingBehavior.SetSource(image, First);
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
            return _platform.RunOnUIThread(async () =>
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

                        return (Bitmap?)_firstBitmap;
                    });

                var (window, image) = CreateShownImage();
                AsyncImageLoadingBehavior.SetSource(image, First);
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
        public Task Reattaching_A_Loaded_Image_Should_Not_Load_Again()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var (window, image) = CreateShownImage();
                AsyncImageLoadingBehavior.SetSource(image, First);
                await WaitUntil(() => image.Source is not null);

                window.Content = null;
                await Settle();
                window.Content = image;
                await Settle();

                Assert.Same(_firstBitmap, image.Source);
                Assert.Single(_imageService.ReceivedCalls());
                window.Close();
            });
        }
    }
}
