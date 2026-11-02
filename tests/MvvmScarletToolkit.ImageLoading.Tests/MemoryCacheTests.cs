using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class MemoryCacheTests : TraceTestBase, IDisposable
{
    private static readonly Uri Uri = new("https://example.com/image.png");
    private static readonly ImageSize Size = new(100, 100);

    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    private static ImageDataMemoryCache CreateDataCache(IMemoryCache memoryCache)
    {
        return new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, memoryCache, new ImageDataMemoryCacheOptions { IsEnabled = true });
    }

    [Fact]
    public async Task ImageCache_Should_Round_Trip()
    {
        var cache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true });
        var image = new TestImage(TestData.Bytes("image"));

        await cache.CacheImageAsync(image, Uri, Size, TestContext.Current.CancellationToken);

        Assert.Same(image, await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
        Assert.Null(await cache.GetImageAsync(Uri, new ImageSize(1, 1), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DataCache_Should_Return_Independent_Streams()
    {
        var cache = CreateDataCache(_memoryCache);
        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        var first = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(first));
        await first.DisposeAsync();

        // reading or disposing one stream must not affect the next consumer
        await using var second = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(second));
    }

    [Fact]
    public async Task DataCache_Should_Not_Depend_On_Source_Stream_Lifetime()
    {
        var cache = CreateDataCache(_memoryCache);

        await using (var source = new MemoryStream(TestData.Bytes("data")))
        {
            await cache.CacheStreamAsync(source, Uri, Size, TestContext.Current.CancellationToken);
        }

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Copy_Non_Memory_Streams_From_Start()
    {
        var cache = CreateDataCache(_memoryCache);
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "file");
        await File.WriteAllTextAsync(path, "data", TestContext.Current.CancellationToken);

        await using (var source = File.OpenRead(path))
        {
            source.Seek(0, SeekOrigin.End);
            await cache.CacheStreamAsync(source, Uri, Size, TestContext.Current.CancellationToken);
        }

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Work_With_Size_Limited_Memory_Cache()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var cache = CreateDataCache(memoryCache);

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task DataCache_Should_Return_Null_Stream_When_Disabled()
    {
        var cache = new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, _memoryCache, new ImageDataMemoryCacheOptions { IsEnabled = false });

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        Assert.Same(Stream.Null, await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken));
    }
}
