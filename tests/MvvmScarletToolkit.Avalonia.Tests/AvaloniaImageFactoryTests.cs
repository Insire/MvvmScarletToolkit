using Avalonia.Media.Imaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.Avalonia.Tests.Util;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    public sealed class AvaloniaImageFactoryTests : TraceTestBase, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly AvaloniaImageFactory _factory;

        public AvaloniaImageFactoryTests()
        {
            _factory = new AvaloniaImageFactory(NullLogger<AvaloniaImageFactory>.Instance, _semaphore);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        [Fact]
        public async Task FromAsync_Should_Decode_Image()
        {
            using var png = TestBitmaps.CreatePng(10, 10);

            using var image = await _factory.FromAsync(png, new ImageSize(10, 10), TestContext.Current.CancellationToken);

            Assert.Equal(10, image.PixelSize.Width);
            Assert.Equal(10, image.PixelSize.Height);
        }

        [Theory]
        [InlineData(400, 200, 100, 100, 100, 50)]
        [InlineData(200, 400, 100, 100, 50, 100)]
        [InlineData(400, 400, 100, 300, 100, 100)]
        [InlineData(300, 300, 150, 150, 150, 150)]
        [InlineData(1000, 3, 100, 100, 100, 1)]
        public async Task FromAsync_Should_Scale_Down_Keeping_Aspect_Ratio(int width, int height, uint requestedWidth, uint requestedHeight, int expectedWidth, int expectedHeight)
        {
            using var png = TestBitmaps.CreatePng(width, height);

            using var image = await _factory.FromAsync(png, new ImageSize(requestedWidth, requestedHeight), TestContext.Current.CancellationToken);

            Assert.Equal(expectedWidth, image.PixelSize.Width);
            Assert.Equal(expectedHeight, image.PixelSize.Height);
        }

        [Theory]
        [InlineData(50, 40, 100, 100)]
        [InlineData(50, 40, 0, 0)]
        [InlineData(50, 40, 0, 10)]
        public async Task FromAsync_Should_Not_Scale_Up_Or_For_Empty_Size(int width, int height, uint requestedWidth, uint requestedHeight)
        {
            using var png = TestBitmaps.CreatePng(width, height);

            using var image = await _factory.FromAsync(png, new ImageSize(requestedWidth, requestedHeight), TestContext.Current.CancellationToken);

            Assert.Equal(width, image.PixelSize.Width);
            Assert.Equal(height, image.PixelSize.Height);
        }

        [Fact]
        public async Task FromAsync_Should_Work_Concurrently_From_Thread_Pool()
        {
            var images = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
            {
                using var png = TestBitmaps.CreatePng(64, 32);
                return await _factory.FromAsync(png, new ImageSize(32, 32), TestContext.Current.CancellationToken);
            }, TestContext.Current.CancellationToken)));

            Assert.All(images, image =>
            {
                Assert.Equal(32, image.PixelSize.Width);
                Assert.Equal(16, image.PixelSize.Height);
                image.Dispose();
            });
            Assert.Equal(1, _semaphore.CurrentCount);
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
            using var source = TestBitmaps.Create(20, 10);

            await _factory.ToAsync(stream, source, TestContext.Current.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            using var image = await _factory.FromAsync(stream, new ImageSize(20, 10), TestContext.Current.CancellationToken);

            Assert.Equal(20, image.PixelSize.Width);
            Assert.Equal(10, image.PixelSize.Height);
        }

        [Fact]
        public async Task ToAsync_Should_Throw_And_Release_Semaphore_When_Saving_Fails()
        {
            using var readOnly = new MemoryStream(new byte[16], writable: false);
            using var source = TestBitmaps.Create(10, 10);

            await Assert.ThrowsAnyAsync<Exception>(() => _factory.ToAsync(readOnly, source, TestContext.Current.CancellationToken));

            Assert.Equal(1, _semaphore.CurrentCount);
        }

        [Fact]
        public async Task FilesystemCache_Should_Round_Trip_And_Drop_Corrupt_Entries()
        {
            var directory = Path.Combine(Path.GetTempPath(), "MvvmScarletToolkit.Avalonia.Tests", Guid.NewGuid().ToString("N"));
            var cache = new ImageFilesystemCache<Bitmap>(
                NullLogger<ImageFilesystemCache<Bitmap>>.Instance,
                _factory,
                new ImageFilesystemCacheOptions { CacheDirectoryPath = directory, CreateFolder = true, IsEnabled = true });
            var uri = new Uri("https://example.com/image.png");
            var size = new ImageSize(20, 10);

            try
            {
                using (var source = TestBitmaps.Create(20, 10))
                {
                    await cache.CacheImageAsync(source, uri, size, TestContext.Current.CancellationToken);
                }

                using (var image = await cache.GetImageAsync(uri, size, TestContext.Current.CancellationToken))
                {
                    Assert.NotNull(image);
                    Assert.Equal(20, image.PixelSize.Width);
                }

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

        [Fact]
        public async Task ImageService_Should_Load_Resize_And_Cache_With_Avalonia_Factory()
        {
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var dataProvider = Substitute.For<IImageDataProvider>();
            dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => Task.FromResult<Stream>(TestBitmaps.CreatePng(200, 100)));
            var service = new ImageService<Bitmap>(
                NullLogger<ImageService<Bitmap>>.Instance,
                _factory,
                dataProvider,
                new ImageDataFileystemCache(NullLogger<ImageDataFileystemCache>.Instance, new ImageDataFileystemCacheOptions()),
                new ImageFilesystemCache<Bitmap>(NullLogger<ImageFilesystemCache<Bitmap>>.Instance, _factory, new ImageFilesystemCacheOptions()),
                new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, memoryCache, new ImageDataMemoryCacheOptions()),
                new ImageMemoryCache<Bitmap>(NullLogger<ImageMemoryCache<Bitmap>>.Instance, memoryCache, new ImageMemoryCacheOptions { IsEnabled = true }),
                memoryCache,
                new ImageServiceOptions());
            var uri = new Uri("https://example.com/image.png");

            var first = await service.ProvideImageAsync(uri, new ImageSize(50, 50), _ => Task.CompletedTask, TestContext.Current.CancellationToken);
            var second = await service.ProvideImageAsync(uri, new ImageSize(50, 50), _ => Task.CompletedTask, TestContext.Current.CancellationToken);

            Assert.NotNull(first);
            Assert.Equal(50, first.PixelSize.Width);
            Assert.Equal(25, first.PixelSize.Height);
            Assert.Same(first, second);
            await dataProvider.ReceivedWithAnyArgs(1).GetStreamAsync(default!, Arg.Any<CancellationToken>());
        }
    }
}
