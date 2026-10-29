using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.IO;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class WpfImageFactoryTests : TraceTestBase, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly WpfImageFactory _factory;

        public WpfImageFactoryTests()
        {
            _factory = new WpfImageFactory(NullLogger<WpfImageFactory>.Instance, _semaphore);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        [Fact]
        public async Task FromAsync_Should_Return_Frozen_Image()
        {
            using var png = TestBitmaps.CreatePng(10, 10);

            var image = await _factory.FromAsync(png, new ImageSize(10, 10), TestContext.Current.CancellationToken);

            Assert.True(image.IsFrozen);
            Assert.Equal(10, image.PixelWidth);
            Assert.Equal(10, image.PixelHeight);
        }

        [Theory]
        [InlineData(400, 200, 100, 100, 100, 50)]
        [InlineData(200, 400, 100, 100, 50, 100)]
        [InlineData(400, 400, 100, 300, 100, 100)]
        [InlineData(300, 300, 150, 150, 150, 150)]
        public async Task FromAsync_Should_Scale_Down_Keeping_Aspect_Ratio(int width, int height, uint requestedWidth, uint requestedHeight, int expectedWidth, int expectedHeight)
        {
            using var png = TestBitmaps.CreatePng(width, height);

            var image = await _factory.FromAsync(png, new ImageSize(requestedWidth, requestedHeight), TestContext.Current.CancellationToken);

            Assert.Equal(expectedWidth, image.PixelWidth);
            Assert.Equal(expectedHeight, image.PixelHeight);
        }

        [Fact]
        public async Task FromAsync_Should_Not_Keep_The_Full_Size_Image_When_Scaling_Down()
        {
            using var png = TestBitmaps.CreatePng(400, 400);

            var image = await _factory.FromAsync(png, new ImageSize(100, 100), TestContext.Current.CancellationToken);

            // a TransformedBitmap references its full size source
            Assert.Multiple(() =>
            {
                Assert.IsNotType<TransformedBitmap>(image);
                Assert.True(image.IsFrozen);
                Assert.Equal(100, image.PixelWidth);
            });
        }

        [Theory]
        [InlineData(50, 40, 100, 100)]
        [InlineData(50, 40, 0, 0)]
        [InlineData(50, 40, 0, 10)]
        public async Task FromAsync_Should_Not_Scale_Up_Or_For_Empty_Size(int width, int height, uint requestedWidth, uint requestedHeight)
        {
            using var png = TestBitmaps.CreatePng(width, height);

            var image = await _factory.FromAsync(png, new ImageSize(requestedWidth, requestedHeight), TestContext.Current.CancellationToken);

            Assert.Equal(width, image.PixelWidth);
            Assert.Equal(height, image.PixelHeight);
        }

        [Fact]
        public async Task FromAsync_Should_Work_When_Stream_Completes_On_Another_Thread()
        {
            using var png = TestBitmaps.CreatePng(10, 10);

            // the image is a DispatcherObject, resuming on a different thread mid-construction must not break it
            var image = await Task.Run(() => _factory.FromAsync(new YieldingStream(png), new ImageSize(10, 10), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

            Assert.Equal(10, image.PixelWidth);
        }

        [Fact]
        public async Task FromAsync_Should_Throw_And_Release_Semaphore_For_Invalid_Data()
        {
            using var invalid = new MemoryStream([1, 2, 3, 4]);

            await Assert.ThrowsAnyAsync<Exception>(() => _factory.FromAsync(invalid, new ImageSize(10, 10), TestContext.Current.CancellationToken));

            Assert.Equal(1, _semaphore.CurrentCount);
        }

        [Fact]
        public async Task FromAsync_Cancelled_While_Waiting_Should_Not_Release_Foreign_Semaphore_Hold()
        {
            await _semaphore.WaitAsync(TestContext.Current.CancellationToken);
            using var cts = new CancellationTokenSource();
            using var png = TestBitmaps.CreatePng(10, 10);

            var pending = _factory.FromAsync(png, new ImageSize(10, 10), cts.Token);
            await cts.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Equal(0, _semaphore.CurrentCount);

            _semaphore.Release();
        }

        [Fact]
        public async Task ToAsync_Should_Round_Trip()
        {
            using var stream = new MemoryStream();

            await _factory.ToAsync(stream, TestBitmaps.Create(20, 10), TestContext.Current.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            var image = await _factory.FromAsync(stream, new ImageSize(20, 10), TestContext.Current.CancellationToken);

            Assert.Equal(20, image.PixelWidth);
            Assert.Equal(10, image.PixelHeight);
        }

        [Fact]
        public async Task ToAsync_Should_Not_Throw_Internally()
        {
            using var stream = new MemoryStream();
            using var png = TestBitmaps.CreatePng(20, 10);
            var decoded = await _factory.FromAsync(png, new ImageSize(20, 10), TestContext.Current.CancellationToken);

            // encoding happens for every image cached on disk, the metadata probe of BitmapFrame.Create(source) used to throw each time
            var exceptions = await FirstChanceExceptions.CaptureAsync(() => _factory.ToAsync(stream, decoded, TestContext.Current.CancellationToken));

            Assert.Empty(exceptions);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToAsync_Should_Throw_And_Release_Semaphore_When_Saving_Fails()
        {
            using var readOnly = new MemoryStream(new byte[16], writable: false);

            await Assert.ThrowsAnyAsync<Exception>(() => _factory.ToAsync(readOnly, TestBitmaps.Create(10, 10), TestContext.Current.CancellationToken));

            Assert.Equal(1, _semaphore.CurrentCount);
        }

        [Fact]
        public async Task FilesystemCache_Should_Round_Trip_With_Wpf_Factory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "MvvmScarletToolkit.Wpf.Tests", Guid.NewGuid().ToString("N"));
            var cache = new ImageFilesystemCache<BitmapSource>(
                NullLogger<ImageFilesystemCache<BitmapSource>>.Instance,
                _factory,
                new ImageFilesystemCacheOptions { CacheDirectoryPath = directory, CreateFolder = true, IsEnabled = true });
            var uri = new Uri("https://example.com/image.png");
            var size = new ImageSize(20, 10);

            try
            {
                await cache.CacheImageAsync(TestBitmaps.Create(20, 10), uri, size, TestContext.Current.CancellationToken);
                var image = await cache.GetImageAsync(uri, size, TestContext.Current.CancellationToken);

                Assert.NotNull(image);
                Assert.Equal(20, image.PixelWidth);

                // corrupt the entry, it must be dropped instead of failing forever
                var file = Assert.Single(Directory.GetFiles(directory));
                await File.WriteAllBytesAsync(file, [1, 2, 3, 4], TestContext.Current.CancellationToken);

                Assert.Null(await cache.GetImageAsync(uri, size, TestContext.Current.CancellationToken));
                Assert.Empty(Directory.GetFiles(directory));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
