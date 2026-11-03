using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.MimeTypeResolvers;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class FileSystemViewModelFactoryTests : TraceTestBase, IDisposable
    {
        private static readonly IReadOnlyCollection<FileAttributes> _fileAttributes = [FileAttributes.Archive, FileAttributes.Normal, FileAttributes.ReadOnly];
        private static readonly IReadOnlyCollection<FileAttributes> _folderAttributes = [FileAttributes.Directory];

        private readonly string _directory;
        private readonly FileSystemViewModelFactory _factory;

        public FileSystemViewModelFactoryTests()
        {
            _directory = Path.Combine(Path.GetTempPath(), nameof(FileSystemViewModelFactoryTests), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);

            _factory = new FileSystemViewModelFactory(NullLogger<FileSystemViewModelFactory>.Instance, ImmediateScheduler.Instance);
        }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }

        private static IFileSystemParent CreateParent(string path)
        {
            var parent = Substitute.For<IFileSystemParent>();
            parent.FullName.Returns(path);

            return parent;
        }

        private string CreateFile(string name, byte[] content)
        {
            var path = Path.Combine(_directory, name);
            File.WriteAllBytes(path, content);

            return path;
        }

        private static byte[] CreatePngBytes()
        {
            using var png = TestBitmaps.CreatePng(1, 1);

            return png.ToArray();
        }

        [Fact]
        public async Task GetFiles_Should_Return_Empty_For_Missing_Directory()
        {
            var files = await _factory.GetFiles(CreateParent(Path.Combine(_directory, "missing")), _fileAttributes, TestContext.Current.CancellationToken);

            Assert.Empty(files);
        }

        [Fact]
        public async Task GetDirectories_Should_Return_Empty_For_Missing_Directory()
        {
            var directories = await _factory.GetDirectories(CreateParent(Path.Combine(_directory, "missing")), _fileAttributes, _folderAttributes, TestContext.Current.CancellationToken);

            Assert.Empty(directories);
        }

        [Fact]
        public async Task GetDirectories_Should_Return_Sub_Directories()
        {
            Directory.CreateDirectory(Path.Combine(_directory, "sub"));

            var directories = await _factory.GetDirectories(CreateParent(_directory), _fileAttributes, _folderAttributes, TestContext.Current.CancellationToken);

            Assert.Equal(["sub"], directories.Select(p => p.Name));
        }

        [Fact]
        public async Task GetFiles_Should_Return_Files_With_MimeType()
        {
            CreateFile("readme.txt", "text"u8.ToArray());

            var files = await _factory.GetFiles(CreateParent(_directory), _fileAttributes, TestContext.Current.CancellationToken);

            var file = Assert.IsType<ScarletFile>(Assert.Single(files));
            Assert.Multiple(() =>
            {
                Assert.Equal("readme.txt", file.Name);
                Assert.Equal("text/plain", file.MimeType);
            });
        }

        [Fact]
        public async Task GetFiles_Should_Prefer_The_Extension_Over_The_Content()
        {
            // the content is not read for known extensions
            CreateFile("image.png", "not a png"u8.ToArray());

            var files = await _factory.GetFiles(CreateParent(_directory), _fileAttributes, TestContext.Current.CancellationToken);

            Assert.Equal("image/png", Assert.IsType<ScarletFile>(Assert.Single(files)).MimeType);
        }

        [Fact]
        public async Task GetFiles_Should_Fall_Back_To_The_Content_For_Unknown_Extensions()
        {
            CreateFile("image.unknownextension", CreatePngBytes());

            var files = await _factory.GetFiles(CreateParent(_directory), _fileAttributes, TestContext.Current.CancellationToken);

            Assert.Equal("image/png", Assert.IsType<ScarletFile>(Assert.Single(files)).MimeType);
        }

        [Theory]
        [InlineData(new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00 })] // ID3v2 tag
        [InlineData(new byte[] { 0xFF, 0xFB, 0x90, 0x64 })] // MPEG-1 layer III, 128 kbit/s
        [InlineData(new byte[] { 0xFF, 0xFB, 0xE0, 0x44 })] // MPEG-1 layer III, 320 kbit/s
        [InlineData(new byte[] { 0xFF, 0xF3, 0x40, 0xC4 })] // MPEG-2 layer III
        public void MagicNumberMimeTypeResolver_Should_Detect_Mp3(byte[] header)
        {
            var path = CreateFile("audio.unknownextension", [.. header, .. new byte[16]]);

            Assert.Equal("audio/mpeg", new MagicNumberMimeTypeResolver().Get(new FileInfo(path)));
        }

        [Fact]
        public async Task IsEmpty_Should_Throw_For_Unsupported_Parent()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _factory.IsEmpty(CreateParent(_directory), TestContext.Current.CancellationToken));

            Assert.Equal("parent", exception.ParamName);
        }

        [Fact]
        public void MagicNumberMimeTypeResolver_Should_Compare_Extensions_Independent_Of_Culture()
        {
            // in turkish "i".ToUpper() is "İ", so ".avi" would not match ".AVI"
            var path = CreateFile("clip.avi", [.. "RIFF"u8, .. new byte[12]]);
            var culture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

                Assert.Equal("video/x-msvideo", new MagicNumberMimeTypeResolver().Get(new FileInfo(path)));
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }
        }
    }
}
