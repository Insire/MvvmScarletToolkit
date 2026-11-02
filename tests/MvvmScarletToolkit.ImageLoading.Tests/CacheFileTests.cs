using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.ImageLoading.Caches;

namespace MvvmScarletToolkit.ImageLoading.Tests;

public sealed class CacheFileTests : TraceTestBase, IDisposable
{
    private readonly TempDirectory _directory = new();

    public void Dispose()
    {
        _directory.Dispose();
    }

    [Fact]
    public void CreateName_Should_Be_Deterministic()
    {
        var uri = new Uri("https://example.com/image.png");
        var size = new ImageSize(100, 200);

        Assert.Equal(CacheFile.CreateName("image", uri, size), CacheFile.CreateName("image", uri, size));
    }

    [Fact]
    public void CreateName_Should_Differ_For_Non_Ascii_Uris()
    {
        var size = new ImageSize(100, 100);

        // ASCII encoding used to turn both into the same "?" sequence
        var first = CacheFile.CreateName("image", new Uri("https://example.com/äöü.png"), size);
        var second = CacheFile.CreateName("image", new Uri("https://example.com/ßéè.png"), size);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void CreateName_Should_Differ_By_Scope_And_Size()
    {
        var uri = new Uri("https://example.com/image.png");

        var names = new[]
        {
            CacheFile.CreateName("image", uri, new ImageSize(100, 100)),
            CacheFile.CreateName("data", uri, new ImageSize(100, 100)),
            CacheFile.CreateName("image", uri, new ImageSize(100, 101)),
        };

        Assert.Equal(names.Length, names.Distinct().Count());
    }

    [Fact]
    public async Task WriteAtomicAsync_Should_Replace_Longer_Content_Completely()
    {
        var path = Path.Combine(_directory.Path, "file");
        await File.WriteAllTextAsync(path, "a much longer previous content", TestContext.Current.CancellationToken);

        await CacheFile.WriteAtomicAsync(path, stream => stream.WriteAsync(TestData.Bytes("short")).AsTask(), TestContext.Current.CancellationToken);

        Assert.Equal("short", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WriteAtomicAsync_Should_Keep_Previous_Content_And_Clean_Up_When_Write_Fails()
    {
        var path = Path.Combine(_directory.Path, "file");
        await File.WriteAllTextAsync(path, "previous", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CacheFile.WriteAtomicAsync(path, async stream =>
        {
            await stream.WriteAsync(TestData.Bytes("partial"));
            throw new InvalidOperationException();
        }, TestContext.Current.CancellationToken));

        Assert.Equal("previous", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
        Assert.Single(Directory.GetFiles(_directory.Path));
    }

    [Fact]
    public async Task WriteAtomicAsync_Should_Reject_Empty_Content()
    {
        var path = Path.Combine(_directory.Path, "file");

        await Assert.ThrowsAsync<InvalidDataException>(() => CacheFile.WriteAtomicAsync(path, _ => Task.CompletedTask, TestContext.Current.CancellationToken));

        Assert.Empty(Directory.GetFiles(_directory.Path));
    }

    [Fact]
    public async Task WriteAtomicAsync_Should_Not_Fail_When_File_Is_Open_For_Reading()
    {
        var path = Path.Combine(_directory.Path, "file");
        await File.WriteAllTextAsync(path, "old", TestContext.Current.CancellationToken);

        await using (var reader = CacheFile.OpenRead(path))
        {
            // replacing an open file works on unix, windows keeps the existing one
            var replaced = await CacheFile.WriteAtomicAsync(path, stream => stream.WriteAsync(TestData.Bytes("new")).AsTask(), TestContext.Current.CancellationToken);

            Assert.Equal("old", await TestData.ReadAllTextAsync(reader));
            Assert.Equal(replaced ? "new" : "old", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
            Assert.Single(Directory.GetFiles(_directory.Path));
        }

        Assert.True(await CacheFile.WriteAtomicAsync(path, stream => stream.WriteAsync(TestData.Bytes("newer")).AsTask(), TestContext.Current.CancellationToken));
        Assert.Equal("newer", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Initialize_Should_Clear_Directory_When_Requested()
    {
        var path = Path.Combine(_directory.Path, "cache");
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "file"), "content");

        CacheFile.Initialize(NullLogger.Instance, path, createFolder: true, clearDirectory: true, maxFileAgeInDays: 0);

        Assert.True(Directory.Exists(path));
        Assert.Empty(Directory.GetFiles(path));
    }

    [Fact]
    public void Initialize_Should_Not_Create_Directory_When_Disabled()
    {
        var path = Path.Combine(_directory.Path, "cache");

        CacheFile.Initialize(NullLogger.Instance, path, createFolder: false, clearDirectory: true, maxFileAgeInDays: 0);

        Assert.False(Directory.Exists(path));
    }
}
