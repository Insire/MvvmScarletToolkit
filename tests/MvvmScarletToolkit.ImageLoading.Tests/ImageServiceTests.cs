using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class ImageServiceTests : TraceTestBase, IDisposable
{
    private static readonly Uri Uri = new("https://example.com/image.png");
    private static readonly ImageSize Size = new(100, 100);
    private static readonly Func<bool, Task> NoCallback = _ => Task.CompletedTask;

    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly TestImageFactory _factory = new();
    private readonly IImageDataProvider _dataProvider = Substitute.For<IImageDataProvider>();
    private readonly IImageFilesystemCache<TestImage> _filesystemCache = Substitute.For<IImageFilesystemCache<TestImage>>();
    private readonly IImageDataFileystemCache _dataFilesystemCache = Substitute.For<IImageDataFileystemCache>();
    private readonly ImageMemoryCache<TestImage> _imageMemoryCache;
    private readonly ImageDataMemoryCache _dataMemoryCache;

    public ImageServiceTests()
    {
        _imageMemoryCache = new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, _memoryCache, new ImageMemoryCacheOptions { IsEnabled = true });
        _dataMemoryCache = new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, _memoryCache, new ImageDataMemoryCacheOptions { IsEnabled = true });

        _filesystemCache.GetImageAsync(default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult<TestImage?>(null));
        _dataFilesystemCache.GetStreamAsync(default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => Task.FromResult(Stream.Null));
        ReturnFromRemote("remote");
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    private ImageService<TestImage> CreateService(ImageServiceOptions? options = null)
    {
        return new ImageService<TestImage>(
            NullLogger<ImageService<TestImage>>.Instance,
            _factory,
            _dataProvider,
            _dataFilesystemCache,
            _filesystemCache,
            _dataMemoryCache,
            _imageMemoryCache,
            _memoryCache,
            options ?? new ImageServiceOptions());
    }

    private void ReturnFromRemote(string content)
    {
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => Task.FromResult<Stream>(new MemoryStream(TestData.Bytes(content))));
    }

    [Fact]
    public async Task Should_Return_Null_For_Null_Uri()
    {
        var service = CreateService();

        Assert.Null(await service.ProvideImageAsync(null, Size, NoCallback, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Return_Null_When_Cancelled_Before_Start()
    {
        var service = CreateService();

        Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, new CancellationToken(true)));
        Assert.Empty(_dataProvider.ReceivedCalls());
    }

    public enum DecodingSource
    {
        Remote,
        MemoryData,
        FilesystemData,
    }

    [Theory]
    [InlineData(DecodingSource.Remote)]
    [InlineData(DecodingSource.MemoryData)]
    [InlineData(DecodingSource.FilesystemData)]
    public async Task Decoding_Should_Never_Run_On_The_Calling_Thread(DecodingSource source)
    {
        switch (source)
        {
            case DecodingSource.MemoryData:
                // completes synchronously, ConfigureAwait(false) alone would keep going on the caller's thread
                await _dataMemoryCache.CacheStreamAsync(new MemoryStream(TestData.Bytes("memory data")), Uri, Size, TestContext.Current.CancellationToken);
                break;

            case DecodingSource.FilesystemData:
                _dataFilesystemCache.GetStreamAsync(Uri, Size, Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult<Stream>(new MemoryStream(TestData.Bytes("disk data"))));
                break;
        }

        var service = CreateService();
        using var uiThread = new SingleThreadContext();

        var resumedOnCallingThread = await uiThread.RunAsync(async () =>
        {
            var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
            Assert.NotNull(image);

            // the caller still continues on its own thread
            return Environment.CurrentManagedThreadId == uiThread.ThreadId;
        });

        Assert.True(resumedOnCallingThread);
        Assert.Equal(1, _factory.FromCalls);
        Assert.DoesNotContain(uiThread.ThreadId, _factory.DecodingThreads);
    }

    [Fact]
    public async Task Memory_Hit_Should_Complete_Synchronously()
    {
        var cached = new TestImage(TestData.Bytes("cached"));
        await _imageMemoryCache.CacheImageAsync(cached, Uri, Size, TestContext.Current.CancellationToken);
        var service = CreateService();

        var pending = service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        // the fast path must not hop threads, cached images show up without delay
        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Same(cached, await pending);
    }

    [Fact]
    public async Task Cancelled_Load_Should_Not_Be_Remembered_As_Failure()
    {
        using var cts = new CancellationTokenSource();
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            // like the real provider, cancellation is reported with an empty stream
            await cts.CancelAsync();
            return Stream.Null;
        });
        var service = CreateService();

        Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, cts.Token));

        ReturnFromRemote("remote");
        Assert.Equal("remote", (await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken))?.Text);
    }

    [Fact]
    public async Task Image_Obtained_Before_Cancellation_Should_Still_Be_Cached()
    {
        using var cts = new CancellationTokenSource();
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            // the download completed, but the image got scrolled away in the meantime
            await cts.CancelAsync();
            return (Stream)new MemoryStream(TestData.Bytes("remote"));
        });
        var service = CreateService();

        await service.ProvideImageAsync(Uri, Size, NoCallback, cts.Token);
        await service.FlushAsync(TestContext.Current.CancellationToken);

        Assert.Equal("remote", (await _imageMemoryCache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken))?.Text);
        await _dataFilesystemCache.ReceivedWithAnyArgs(1).CacheStreamAsync(default!, default!, default, Arg.Any<CancellationToken>());
        await _filesystemCache.ReceivedWithAnyArgs(1).CacheImageAsync(default!, default!, default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancellation_Should_Not_Throw_Exceptions()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            entered.TrySetResult();
            await gate.Task;
            return (Stream)new MemoryStream(TestData.Bytes("remote"));
        });
        var service = CreateService();
        var holder = service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);

        // cancelled before the start and while waiting for the lock of the same image
        var exceptions = await FirstChanceExceptions.CaptureAsync(async () =>
        {
            Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, new CancellationToken(true)));

            using var cts = new CancellationTokenSource();
            var waiting = service.ProvideImageAsync(Uri, Size, NoCallback, cts.Token);
            await cts.CancelAsync();
            Assert.Null(await waiting);
        });

        gate.SetResult();
        await holder;

        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Should_Load_From_Remote_And_Populate_All_Caches()
    {
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Equal("remote", image?.Text);
        Assert.Same(image, await _imageMemoryCache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
        await using (var cachedData = await _dataMemoryCache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken))
        {
            Assert.Equal("remote", await TestData.ReadAllTextAsync(cachedData));
        }

        await service.FlushAsync(TestContext.Current.CancellationToken);
        await _dataFilesystemCache.ReceivedWithAnyArgs(1).CacheStreamAsync(default!, default!, default, Arg.Any<CancellationToken>());
        await _filesystemCache.Received(1).CacheImageAsync(image!, Uri, Size, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Use_Default_Size_When_None_Requested()
    {
        var service = CreateService(new ImageServiceOptions { DefaultWidth = 42, DefaultHeight = 24 });

        var image = await service.ProvideImageAsync(Uri, null, NoCallback, TestContext.Current.CancellationToken);

        Assert.Same(image, await _imageMemoryCache.GetImageAsync(Uri, new ImageSize(42, 24), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Memory_Hit_Should_Not_Touch_Disk_Or_Remote()
    {
        var cached = new TestImage(TestData.Bytes("cached"));
        await _imageMemoryCache.CacheImageAsync(cached, Uri, Size, TestContext.Current.CancellationToken);
        var callbackCalls = 0;
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, _ => { callbackCalls++; return Task.CompletedTask; }, TestContext.Current.CancellationToken);

        Assert.Same(cached, image);
        Assert.Equal(0, callbackCalls);
        Assert.Empty(_filesystemCache.ReceivedCalls());
        Assert.Empty(_dataFilesystemCache.ReceivedCalls());
        Assert.Empty(_dataProvider.ReceivedCalls());
    }

    [Fact]
    public async Task Memory_Data_Hit_Should_Return_Without_Further_Lookups()
    {
        await _dataMemoryCache.CacheStreamAsync(new MemoryStream(TestData.Bytes("memory data")), Uri, Size, TestContext.Current.CancellationToken);
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Equal("memory data", image?.Text);
        Assert.Empty(_dataProvider.ReceivedCalls());
        await _filesystemCache.DidNotReceiveWithAnyArgs().GetImageAsync(default!, default, Arg.Any<CancellationToken>());
        await _dataFilesystemCache.DidNotReceiveWithAnyArgs().GetStreamAsync(default!, default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Corrupt_Memory_Data_Should_Fall_Through_To_Remote()
    {
        await _dataMemoryCache.CacheStreamAsync(new MemoryStream(TestData.Bytes("corrupt")), Uri, Size, TestContext.Current.CancellationToken);
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Equal("remote", image?.Text);
    }

    [Fact]
    public async Task Filesystem_Image_Hit_Should_Populate_Memory_Without_Remote()
    {
        var cached = new TestImage(TestData.Bytes("disk"));
        _filesystemCache.GetImageAsync(Uri, Size, Arg.Any<CancellationToken>()).Returns(Task.FromResult<TestImage?>(cached));
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Same(cached, image);
        Assert.Same(cached, await _imageMemoryCache.GetImageAsync(Uri, Size, TestContext.Current.CancellationToken));
        Assert.Empty(_dataProvider.ReceivedCalls());
    }

    [Fact]
    public async Task Filesystem_Data_Hit_Should_Dispose_File_Stream()
    {
        var fileStream = new MemoryStream(TestData.Bytes("disk data"));
        _dataFilesystemCache.GetStreamAsync(Uri, Size, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Stream>(fileStream));
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Equal("disk data", image?.Text);
        Assert.False(fileStream.CanRead);
        Assert.Empty(_dataProvider.ReceivedCalls());

        // the memory copy must outlive the disposed file stream
        await using var cachedData = await _dataMemoryCache.GetStreamAsync(Uri, Size, TestContext.Current.CancellationToken);
        Assert.Equal("disk data", await TestData.ReadAllTextAsync(cachedData));
    }

    [Fact]
    public async Task Concurrent_Requests_Should_Load_Only_Once()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            entered.TrySetResult();
            await gate.Task;
            return (Stream)new MemoryStream(TestData.Bytes("remote"));
        });
        var service = CreateService();

        var first = service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);
        var others = Enumerable.Range(0, 10).Select(_ => service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken)).ToArray();

        gate.SetResult();
        var images = await Task.WhenAll(others.Prepend(first));

        Assert.All(images, image => Assert.Same(images[0], image));
        await _dataProvider.ReceivedWithAnyArgs(1).GetStreamAsync(default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelled_Waiter_Should_Not_Break_Mutual_Exclusion()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            entered.TrySetResult();
            await gate.Task;
            return (Stream)new MemoryStream(TestData.Bytes("remote"));
        });
        var service = CreateService();

        var holder = service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        var cancelled = service.ProvideImageAsync(Uri, Size, NoCallback, cts.Token);
        await cts.CancelAsync();
        Assert.Null(await cancelled);

        // must still wait for the holder instead of entering because of a bogus release
        var waiter = service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
        Assert.False(waiter.IsCompleted);

        gate.SetResult();

        Assert.Same(await holder, await waiter);
        await _dataProvider.ReceivedWithAnyArgs(1).GetStreamAsync(default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Failed_Load_Should_Be_Remembered()
    {
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => Task.FromResult(Stream.Null));
        var service = CreateService();

        Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken));
        Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken));

        await _dataProvider.ReceivedWithAnyArgs(1).GetStreamAsync(default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Failed_Load_Should_Be_Retried_When_Remembering_Is_Disabled()
    {
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => Task.FromResult(Stream.Null));
        var service = CreateService(new ImageServiceOptions { FailedLoadRetryDelayInSeconds = 0 });

        await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);
        await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        await _dataProvider.ReceivedWithAnyArgs(2).GetStreamAsync(default!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Undecodable_Remote_Data_Should_Return_Null()
    {
        ReturnFromRemote("corrupt");
        var service = CreateService();

        Assert.Null(await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken));
        await service.FlushAsync(TestContext.Current.CancellationToken);
        await _filesystemCache.DidNotReceiveWithAnyArgs().CacheImageAsync(default!, default!, default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throwing_Callback_Should_Not_Fail_The_Load()
    {
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, _ => throw new InvalidOperationException(), TestContext.Current.CancellationToken);

        Assert.Equal("remote", image?.Text);
    }

    [Fact]
    public async Task Should_Notify_Slow_Load_After_Memory_Miss()
    {
        var notifications = new List<bool>();
        var service = CreateService();

        await service.ProvideImageAsync(Uri, Size, value => { notifications.Add(value); return Task.CompletedTask; }, TestContext.Current.CancellationToken);

        Assert.Equal([true], notifications);
    }

    [Fact]
    public async Task Remote_Stream_Should_Be_Disposed_Once_After_The_Disk_Cache_Write()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var remoteStream = new DisposalCountingStream(TestData.Bytes("remote"));
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult<Stream>(remoteStream));
        _dataFilesystemCache.CacheStreamAsync(default!, default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async call =>
        {
            await gate.Task;

            // still usable, the background write owns it
            Assert.Equal("remote", await TestData.ReadAllTextAsync(call.Arg<Stream>()));
        });
        var service = CreateService();

        await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(0, remoteStream.DisposeCount);

        gate.SetResult();
        await service.FlushAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, remoteStream.DisposeCount);
    }

    [Fact]
    public async Task Remote_Stream_Should_Be_Disposed_When_It_Cannot_Be_Decoded()
    {
        var remoteStream = new DisposalCountingStream(TestData.Bytes("corrupt"));
        _dataProvider.GetStreamAsync(default!, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult<Stream>(remoteStream));
        var service = CreateService();

        await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        // nothing gets handed over, so it is disposed right away
        Assert.Equal(1, remoteStream.DisposeCount);
    }

    [Fact]
    public async Task Image_Should_Be_Returned_Before_The_Disk_Caches_Are_Written()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _filesystemCache.CacheImageAsync(default!, default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => gate.Task);
        _dataFilesystemCache.CacheStreamAsync(default!, default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => gate.Task);
        var service = CreateService();

        var image = await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal("remote", image?.Text);

        var flush = service.FlushAsync(TestContext.Current.CancellationToken);
        Assert.False(flush.IsCompleted);

        gate.SetResult();
        await flush;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Disk_Cache_Writes_Should_Be_Limited(int limit)
    {
        var running = 0;
        var maxRunning = 0;
        _filesystemCache.CacheImageAsync(default!, default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async _ =>
        {
            var current = Interlocked.Increment(ref running);
            InterlockedMax(ref maxRunning, current);
            await Task.Delay(20);
            Interlocked.Decrement(ref running);
        });
        var service = CreateService(new ImageServiceOptions { MaxConcurrentDiskCacheWrites = limit });

        await Task.WhenAll(Enumerable.Range(0, 8).Select(i => service.ProvideImageAsync(new Uri($"https://example.com/{i}.png"), Size, NoCallback, TestContext.Current.CancellationToken)));
        await service.FlushAsync(TestContext.Current.CancellationToken);

        Assert.Equal(8, _filesystemCache.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IImageFilesystemCache<TestImage>.CacheImageAsync)));
        Assert.InRange(maxRunning, 1, limit);

        static void InterlockedMax(ref int target, int value)
        {
            int initial;
            do
            {
                initial = target;
                if (value <= initial)
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref target, value, initial) != initial);
        }
    }

    [Fact]
    public async Task Failing_Disk_Cache_Write_Should_Not_Affect_Loading_Or_Flushing()
    {
        _filesystemCache.CacheImageAsync(default!, default!, default, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(Task.FromException(new IOException("disk full")));
        var service = CreateService();

        Assert.Equal("remote", (await service.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken))?.Text);

        await service.FlushAsync(TestContext.Current.CancellationToken);
    }

    private sealed class DisposalCountingStream(byte[] data) : MemoryStream(data)
    {
        public int DisposeCount { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }

            base.Dispose(disposing);
        }
    }

    [Fact]
    public async Task Should_Work_With_Real_Filesystem_Caches_Across_Instances()
    {
        using var directory = new TempDirectory();
        var imageOptions = new ImageFilesystemCacheOptions { CacheDirectoryPath = Path.Combine(directory.Path, "encoded"), CreateFolder = true, IsEnabled = true };
        var dataOptions = new ImageDataFileystemCacheOptions { CacheDirectoryPath = Path.Combine(directory.Path, "raw"), CreateFolder = true, IsEnabled = true };

        ImageService<TestImage> Create(IMemoryCache memoryCache) => new(
            NullLogger<ImageService<TestImage>>.Instance,
            _factory,
            _dataProvider,
            new ImageDataFileystemCache(NullLogger<ImageDataFileystemCache>.Instance, dataOptions),
            new ImageFilesystemCache<TestImage>(NullLogger<ImageFilesystemCache<TestImage>>.Instance, _factory, imageOptions),
            new ImageDataMemoryCache(NullLogger<ImageDataMemoryCache>.Instance, memoryCache, new ImageDataMemoryCacheOptions { IsEnabled = true }),
            new ImageMemoryCache<TestImage>(NullLogger<ImageMemoryCache<TestImage>>.Instance, memoryCache, new ImageMemoryCacheOptions { IsEnabled = true }),
            memoryCache,
            new ImageServiceOptions());

        using (var firstCache = new MemoryCache(new MemoryCacheOptions()))
        {
            var first = Create(firstCache);
            Assert.Equal("remote", (await first.ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken))?.Text);

            // like an application should before exiting
            await first.FlushAsync(TestContext.Current.CancellationToken);
        }

        // simulates an application restart: nothing in memory, everything on disk
        using var secondCache = new MemoryCache(new MemoryCacheOptions());
        var image = await Create(secondCache).ProvideImageAsync(Uri, Size, NoCallback, TestContext.Current.CancellationToken);

        Assert.Equal("remote", image?.Text);
        await _dataProvider.ReceivedWithAnyArgs(1).GetStreamAsync(default!, Arg.Any<CancellationToken>());
        Assert.Single(Directory.GetFiles(imageOptions.CacheDirectoryPath));
        Assert.Single(Directory.GetFiles(dataOptions.CacheDirectoryPath));
    }
}
