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
    public async Task ImageCache_Should_Remove_Least_Recently_Used_Images_Above_MaxEntries()
    {
        var cache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true, MaxEntries = 2 });
        var first = new Uri("https://example.com/1.png");
        var second = new Uri("https://example.com/2.png");
        var third = new Uri("https://example.com/3.png");

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("1")), first, Size, TestContext.Current.CancellationToken);
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("2")), second, Size, TestContext.Current.CancellationToken);

        // reading the first image makes the second one the least recently used
        Assert.NotNull(await cache.GetImageAsync(first, Size, TestContext.Current.CancellationToken));
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("3")), third, Size, TestContext.Current.CancellationToken);

        Assert.NotNull(await cache.GetImageAsync(first, Size, TestContext.Current.CancellationToken));
        Assert.Null(await cache.GetImageAsync(second, Size, TestContext.Current.CancellationToken));
        Assert.NotNull(await cache.GetImageAsync(third, Size, TestContext.Current.CancellationToken));
        Assert.Equal(2, _memoryCache.Count);
    }

    [Fact]
    public async Task ImageCache_Should_Not_Count_Replaced_Images_Twice()
    {
        var cache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true, MaxEntries = 2 });
        var other = new Uri("https://example.com/other.png");

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("other")), other, Size, TestContext.Current.CancellationToken);
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("old")), Uri, Size, TestContext.Current.CancellationToken);
        var image = new TestImage(TestData.Bytes("new"));
        await cache.CacheImageAsync(image, Uri, Size, TestContext.Current.CancellationToken);

        Assert.NotNull(await cache.GetImageAsync(other, Size, TestContext.Current.CancellationToken));
        Assert.Same(image, await cache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ImageCache_Should_Only_Remove_Its_Own_Entries_From_A_Shared_Memory_Cache()
    {
        var cache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true, MaxEntries = 1 });
        _memoryCache.Set("unrelated", "value");

        await cache.CacheImageAsync(new TestImage(TestData.Bytes("1")), new Uri("https://example.com/1.png"), Size, TestContext.Current.CancellationToken);
        await cache.CacheImageAsync(new TestImage(TestData.Bytes("2")), new Uri("https://example.com/2.png"), Size, TestContext.Current.CancellationToken);

        Assert.Equal("value", _memoryCache.Get<string>("unrelated"));
        Assert.Equal(2, _memoryCache.Count);
    }

    [Fact]
    public async Task ImageCache_Should_Not_Limit_Entries_When_MaxEntries_Is_Zero()
    {
        var cache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true, MaxEntries = 0 });

        for (var i = 0; i < 5; i++)
        {
            await cache.CacheImageAsync(new TestImage(TestData.Bytes($"{i}")), new Uri($"https://example.com/{i}.png"), Size, TestContext.Current.CancellationToken);
        }

        Assert.Equal(5, _memoryCache.Count);
    }

    [Fact]
    public async Task DataCache_Should_Remove_Least_Recently_Used_Data_Above_MaxEntries()
    {
        var cache = new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, _memoryCache, new ImageDataMemoryCacheOptions { IsEnabled = true, MaxEntries = 1 });
        var other = new Uri("https://example.com/other.png");

        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("old")), other, Size, TestContext.Current.CancellationToken);
        await cache.CacheStreamAsync(new MemoryStream(TestData.Bytes("data")), Uri, Size, TestContext.Current.CancellationToken);

        Assert.Same(Stream.Null, await cache.GetStreamAsync(other, Size, TestContext.Current.CancellationToken));
        await using var stream = await cache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("data", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public void EntryLimit_Should_Untrack_Entries_Evicted_By_The_Memory_Cache()
    {
        var limit = new MemoryCacheEntryLimit(_memoryCache, 2);
        using (var entry = _memoryCache.CreateEntry("key"))
        {
            entry.Value = "value";
            limit.Add(entry);
        }

        Assert.Equal(1, limit.Count);

        _memoryCache.Remove("key");

        // eviction callbacks run in the background
        Assert.True(SpinWait.SpinUntil(() => limit.Count == 0, TimeSpan.FromSeconds(5)));
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
