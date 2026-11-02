using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using System.Net;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class ImageDataProviderTests : TraceTestBase
{
    private static readonly Uri Uri = new("https://example.com/image.png");

    private static ImageDataProvider CreateProvider(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler, ImageDataProviderOptions? options = null)
    {
        return new ImageDataProvider(
            NullLogger<ImageDataProvider>.Instance,
            new RecyclableMemoryStreamManager(),
            new HttpClient(new StubHttpMessageHandler(handler)),
            options);
    }

    private static Task<HttpResponseMessage> Respond(HttpContent content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Task.FromResult(new HttpResponseMessage(statusCode) { Content = content });
    }

    [Fact]
    public async Task Should_Return_Downloaded_Content_From_Start()
    {
        var provider = CreateProvider((_, _) => Respond(new ByteArrayContent(TestData.Bytes("image"))));

        await using var stream = await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken);

        Assert.Equal(0, stream.Position);
        Assert.Equal("image", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_For_Unsuccessful_Status()
    {
        var provider = CreateProvider((_, _) => Respond(new StringContent("not found"), HttpStatusCode.NotFound));

        var exceptions = await FirstChanceExceptions.CaptureAsync(async () => Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken)));

        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Cancelled_Download_Should_Not_Rethrow_Through_The_Provider()
    {
        using var cts = new CancellationTokenSource();
        var source = new BlockingReadStream();
        var provider = CreateProvider((_, _) => Respond(new StreamContent(source)));

        var exceptions = await FirstChanceExceptions.CaptureAsync(async () =>
        {
            var pending = provider.GetStreamAsync(Uri, cts.Token);
            await source.ReadStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
            await cts.CancelAsync();

            Assert.Same(Stream.Null, await pending);
        });

        Assert.Empty(exceptions);
    }

    /// <summary>
    /// A response body that never ends, its read is cancelled like a network read.
    /// </summary>
    private sealed class BlockingReadStream : Stream
    {
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadStarted.TrySetResult();

            // completes as cancelled without throwing anything itself, so every exception captured comes from the provider
            var never = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellationToken.Register(() => never.TrySetCanceled(cancellationToken));

            return new ValueTask<int>(never.Task);
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Should_Return_Null_Stream_For_Empty_Content()
    {
        var provider = CreateProvider((_, _) => Respond(new ByteArrayContent([])));

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Reject_Content_Length_Above_Limit()
    {
        var provider = CreateProvider((_, _) => Respond(new ByteArrayContent(new byte[100])), new ImageDataProviderOptions { MaxDownloadSizeInBytes = 10 });

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Reject_Streamed_Content_Above_Limit()
    {
        var provider = CreateProvider((_, _) => Respond(new StreamContent(new NonSeekableStream(new byte[100]))), new ImageDataProviderOptions { MaxDownloadSizeInBytes = 10 });

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_On_Timeout()
    {
        var provider = CreateProvider(async (_, token) =>
        {
            await Task.Delay(Timeout.Infinite, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }, new ImageDataProviderOptions { RequestTimeout = TimeSpan.FromMilliseconds(50) });

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_When_Cancelled()
    {
        using var cts = new CancellationTokenSource();
        var provider = CreateProvider(async (_, token) =>
        {
            await cts.CancelAsync();
            await Task.Delay(Timeout.Infinite, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, cts.Token));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_For_Network_Errors()
    {
        var provider = CreateProvider((_, _) => throw new HttpRequestException("network down"));

        Assert.Same(Stream.Null, await provider.GetStreamAsync(Uri, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Read_File_Uri()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "image file.png");
        await File.WriteAllTextAsync(path, "image", TestContext.Current.CancellationToken);
        var provider = CreateProvider((_, _) => throw new InvalidOperationException("no http expected"));

        // "file:///.../image%20file.png", its OriginalString is not a valid file system path
        var uri = new Uri(new Uri(path).AbsoluteUri);

        await using var stream = await provider.GetStreamAsync(uri, TestContext.Current.CancellationToken);

        Assert.Equal("image", await TestData.ReadAllTextAsync(stream));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_For_Missing_File()
    {
        using var directory = new TempDirectory();
        var provider = CreateProvider((_, _) => throw new InvalidOperationException("no http expected"));

        Assert.Same(Stream.Null, await provider.GetStreamAsync(new Uri(Path.Combine(directory.Path, "missing.png")), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Return_Null_Stream_For_Unsupported_Scheme()
    {
        var provider = CreateProvider((_, _) => throw new InvalidOperationException("no http expected"));

        Assert.Same(Stream.Null, await provider.GetStreamAsync(new Uri("ftp://example.com/image.png"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_Use_Child_Implementation_For_Other_Schemes()
    {
        var provider = new CustomSchemeImageDataProvider();

        await using var stream = await provider.GetStreamAsync(new Uri("custom://image"), TestContext.Current.CancellationToken);

        Assert.Equal("custom", await TestData.ReadAllTextAsync(stream));
    }

    private sealed class CustomSchemeImageDataProvider() : ImageDataProvider(NullLogger<ImageDataProvider>.Instance, new RecyclableMemoryStreamManager(), new HttpClient())
    {
        protected override Task<Stream> GetImageStreamFromChildImplementation(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new MemoryStream(TestData.Bytes("custom")));
        }
    }
}
