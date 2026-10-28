using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class FilesystemCacheTests : TraceTestBase, IDisposable
{
    private static readonly Uri Uri = new("https://example.com/image.png");
    private static readonly ImageSize Size = new(100, 100);

    private readonly TempDirectory _directory = new();
    private readonly TestImageFactory _factory = new();

    public void Dispose()
    {
        _directory.Dispose();
    }

    private ImageFilesystemCache<TestImage> CreateImageCache(bool isEnabled = true)
    {
        return new ImageFilesystemCache<TestImage>(
            NullLogger<ImageFilesystemCache<TestImage>>.Instance,
            _factory,
            new ImageFilesystemCacheOptions { CacheDirectoryPath = _directory.Path, CreateFolder = true, IsEnabled = isEnabled });
    }

    private ImageDataFileystemCache CreateDataCache(string? path = null)
    {
        return new ImageDataFileystemCache(
            NullLogger<ImageDataFileystemCache>.Instance,
            new ImageDataFileystemCacheOptions { CacheDirectoryPath = path ?? _directory.Path, CreateFolder = path is null, IsEnabled = true });
    }

    [Fact]
    public async Task ImageCache_Should_Round_Trip()
    {
        var cache = CreateImageCache();

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("image")), Uri, Size, TestContext.Current.CancellationToken);
        var image = await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken);

        Assert.NotNull(image);
        Assert.Equal("image", image.Text);
    }

    [Fact]
    public async Task ImageCache_Should_Return_Null_When_Missing()
    {
        var cache = CreateImageCache();

        // a miss is the common case, it must not cost an exception
        var exceptions = await FirstChanceExceptions.CaptureAsync(async () => Assert.Null(await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken)));

        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task ImageCache_Should_Return_Null_When_Cancelled_During_Decoding()
    {
        var cache = CreateImageCache();
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("image")), Uri, Size, TestContext.Current.CancellationToken);
        var factory = Substitute.For<IImageFactory<TestImage>>();
        factory.FromAsync(default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromCanceled<TestImage>(new CancellationToken(true)));
        var cancellingCache = new ImageFilesystemCache<TestImage>(
            NullLogger<ImageFilesystemCache<TestImage>>.Instance,
            factory,
            new ImageFilesystemCacheOptions { CacheDirectoryPath = _directory.Path, IsEnabled = true });

        Assert.Null(await cancellingCache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));

        // a cancellation is no sign of a corrupt entry
        Assert.Single(Directory.GetFiles(_directory.Path));
    }

    [Fact]
    public async Task ImageCache_Should_Do_Nothing_When_Disabled()
    {
        var cache = CreateImageCache(isEnabled: false);

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("image")), Uri, Size, TestContext.Current.CancellationToken);

        Assert.Null(await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
        Assert.Equal(0, _factory.ToCalls);
    }

    [Fact]
    public async Task ImageCache_Should_Not_Encode_Again_When_File_Exists()
    {
        var cache = CreateImageCache();

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("first")), Uri, Size, TestContext.Current.CancellationToken);
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("second")), Uri, Size, TestContext.Current.CancellationToken);

        Assert.Equal(1, _factory.ToCalls);
    }

    [Fact]
    public async Task ImageCache_Should_Remove_Corrupt_Entry()
    {
        var cache = CreateImageCache();

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("corrupt")), Uri, Size, TestContext.Current.CancellationToken);
        Assert.Single(Directory.GetFiles(_directory.Path));

        Assert.Null(await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
        Assert.Empty(Directory.GetFiles(_directory.Path));
    }

    [Fact]
    public async Task ImageCache_Should_Allow_Concurrent_Reads()
    {
        var cache = CreateImageCache();
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("image")), Uri, Size, TestContext.Current.CancellationToken);

        var images = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken)));

        Assert.All(images, image => Assert.Equal("image", image?.Text));
    }

    [Fact]
    public async Task DataCache_Should_Round_Trip()
    {
        var cache = CreateDataCache();

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Overwrite_With_Shorter_Content()
    {
        var cache = CreateDataCache();

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("a much longer previous content")), Uri, Size, TestContext.Current.CancellationToken);
        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("short")), Uri, Size, TestContext.Current.CancellationToken);

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("short", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Copy_From_Start_Of_Stream()
    {
        var cache = CreateDataCache();
        var source = new MemoryStream(TestData.Bytes("data"));
        source.Seek(0, SeekOrigin.End);

        await cache.CacheStreamAsync(source, Uri, Size, TestContext.Current.CancellationToken);

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Return_Null_Stream_When_Missing()
    {
        var cache = CreateDataCache();

        // a miss is the common case, it must not cost an exception
        var exceptions = await FirstChanceExceptions.CaptureAsync(async () => Assert.Same(Stream.Null, await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken)));

        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task DataCache_Should_Swallow_Write_Failures()
    {
        var cache = CreateDataCache(Path.Combine(_directory.Path, "does", "not", "exist"));

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        Assert.Same(Stream.Null, await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken));
    }
}
