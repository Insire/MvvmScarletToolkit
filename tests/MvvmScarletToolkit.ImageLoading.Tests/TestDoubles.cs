using System.Text;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class TestImage(byte[] data)
{
    public byte[] Data { get; } = data;

    public string Text => Encoding.UTF8.GetString(Data);
}

/// <summary>
/// Turns bytes into a <see cref="TestImage"/> and back. Content starting with "corrupt" fails to decode.
/// </summary>
public sealed class TestImageFactory : IImageFactory<TestImage>
{
    private int _fromCalls;
    private int _toCalls;

    public int FromCalls => _fromCalls;

    public int ToCalls => _toCalls;

    /// <summary>The threads <see cref="FromAsync"/> started on.</summary>
    public System.Collections.Concurrent.ConcurrentQueue<int> DecodingThreads { get; } = new();

    public async Task<TestImage> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _fromCalls);
        DecodingThreads.Enqueue(Environment.CurrentManagedThreadId);

        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy, cancellationToken);

        var bytes = copy.ToArray();
        if (Encoding.UTF8.GetString(bytes).StartsWith("corrupt", StringComparison.Ordinal))
        {
            throw new InvalidDataException("corrupt image");
        }

        return new TestImage(bytes);
    }

    public async Task ToAsync(Stream stream, TestImage image, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _toCalls);

        await stream.WriteAsync(image.Data, cancellationToken);
    }
}

public sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return handler(request, cancellationToken);
    }
}

/// <summary>
/// Hides the length of the content, like a chunked http response.
/// </summary>
public sealed class NonSeekableStream(byte[] data) : Stream
{
    private readonly MemoryStream _inner = new(data);

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

public sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MvvmScarletToolkit.ImageLoading.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch
        {
            // best effort
        }
    }
}

internal static class TestData
{
    public static byte[] Bytes(string value) => Encoding.UTF8.GetBytes(value);

    public static async Task<string> ReadAllTextAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

        return await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
    }
}
