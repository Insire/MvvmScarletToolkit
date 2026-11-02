using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Security.AccessControl;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class FileSystemViewModelTests : TraceTestBase, IDisposable
    {
        private static readonly FileAttributes[] _fileAttributes = [FileAttributes.Archive, FileAttributes.Normal];
        private static readonly FileAttributes[] _folderAttributes = [FileAttributes.Directory];

        private readonly string _directory;
        private readonly FileSystemViewModelFactory _factory;

        public FileSystemViewModelTests()
        {
            _directory = Path.Combine(Path.GetTempPath(), nameof(FileSystemViewModelTests), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);

            _factory = new FileSystemViewModelFactory(NullLogger<FileSystemViewModelFactory>.Instance, ImmediateScheduler.Instance);
        }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }

        private string CreateFile(string relativePath, byte[] content)
        {
            var path = Path.Combine(_directory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, content);

            return path;
        }

        private static byte[] CreatePngBytes()
        {
            using var png = TestBitmaps.CreatePng(1, 1);

            return png.ToArray();
        }

        private IFileSystemParent CreateParent()
        {
            var parent = Substitute.For<IFileSystemParent>();
            parent.FullName.Returns(_directory);
            parent.Name.Returns(Path.GetFileName(_directory));

            return parent;
        }

        private async Task<ScarletFile> GetFile(string name)
        {
            var files = await _factory.GetFiles(CreateParent(), _fileAttributes, TestContext.Current.CancellationToken);

            return files.OfType<ScarletFile>().Single(p => p.Name == name);
        }

        private async Task<(FileSystemViewModel ViewModel, ScarletDirectory Root)> CreateViewModel()
        {
            var parent = CreateParent();
            var info = await _factory.GetDirectoryInfo(_directory, TestContext.Current.CancellationToken);
            var root = new ScarletDirectory(ImmediateScheduler.Instance, info!, _fileAttributes, _folderAttributes, parent, _factory);

            await root.RefreshCommand.ExecuteAsync(null);

            var viewModel = new FileSystemViewModel(ImmediateScheduler.Instance, _factory, new FileSystemOptionsViewModel([], _fileAttributes, _folderAttributes));
            await viewModel.RefreshCommand.ExecuteAsync(null);

            viewModel.OnContainerSelected(root);

            return (viewModel, root);
        }

        private static async Task WaitFor(Func<bool> condition)
        {
            for (var i = 0; i < 200 && !condition(); i++)
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);
            }

            Assert.True(condition());
        }

        [Fact]
        public async Task File_Should_Not_Be_An_Image_Before_Its_Header_Is_Checked()
        {
            CreateFile("image.png", CreatePngBytes());

            var file = await GetFile("image.png");

            Assert.Multiple(() =>
            {
                Assert.Equal("image/png", file.MimeType);
                Assert.False(file.IsImage);
                Assert.False(file.IsHeaderChecked);
            });
        }

        [Fact]
        public async Task CheckHeader_Should_Confirm_An_Image()
        {
            CreateFile("image.png", CreatePngBytes());
            var file = await GetFile("image.png");

            await file.CheckHeaderCommand.ExecuteAsync(null);

            Assert.Multiple(() =>
            {
                Assert.True(file.IsImage);
                Assert.True(file.IsHeaderChecked);
                Assert.Equal("PNG image", file.TypeName);
            });
        }

        [Fact]
        public async Task CheckHeader_Should_Override_The_Extension()
        {
            CreateFile("thumbnail.txt", CreatePngBytes());
            var file = await GetFile("thumbnail.txt");

            await file.CheckHeaderCommand.ExecuteAsync(null);

            Assert.Multiple(() =>
            {
                Assert.True(file.IsImage);
                Assert.Equal("image/png", file.MimeType);
            });
        }

        [Fact]
        public async Task CheckHeader_Should_Reject_Content_That_Is_No_Image()
        {
            CreateFile("setup.png", [.. "MZ"u8, .. new byte[32]]);
            CreateFile("corrupt.png", "not a png"u8.ToArray());
            var executable = await GetFile("setup.png");
            var corrupt = await GetFile("corrupt.png");

            await executable.CheckHeaderCommand.ExecuteAsync(null);
            await corrupt.CheckHeaderCommand.ExecuteAsync(null);

            Assert.Multiple(() =>
            {
                Assert.False(executable.IsImage);
                Assert.Equal("Windows executable", executable.TypeName);
                Assert.False(corrupt.IsImage);
                Assert.Equal("image/png", corrupt.MimeType);
            });
        }

        [Theory]
        [InlineData("a.webp", "RIFF\0\0\0\0WEBP", "image/webp")]
        [InlineData("a.heic", "\0\0\0\u0018ftypheic", "image/heic")]
        [InlineData("a.mp4", "\0\0\0 ftypisom", "video/mp4")]
        public async Task GetFileHeader_Should_Detect_Signatures(string name, string content, string expected)
        {
            var path = CreateFile(name, System.Text.Encoding.ASCII.GetBytes(content));

            var header = await _factory.GetFileHeader(path, TestContext.Current.CancellationToken);

            Assert.Equal(expected, header.MimeType);
        }

        [Fact]
        public async Task GetFileHeader_Should_Report_Unreadable_Files()
        {
            var header = await _factory.GetFileHeader(Path.Combine(_directory, "missing.png"), TestContext.Current.CancellationToken);

            Assert.False(header.IsReadable);
        }

        [Fact]
        public async Task File_Should_Expose_Its_Metadata()
        {
            var path = CreateFile("data.bin", new byte[42]);

            var file = await GetFile("data.bin");

            Assert.Multiple(() =>
            {
                Assert.Equal(42, file.Length);
                Assert.Equal(_directory, file.Location);
                Assert.True(file.Attributes.HasFlag(FileAttributes.Archive));
            });
        }

        [Fact]
        public async Task GetPermissions_Should_Return_Owner_And_Rules()
        {
            var path = CreateFile("data.bin", new byte[1]);

            var permissions = await _factory.GetPermissions(path, false, TestContext.Current.CancellationToken);

            Assert.NotNull(permissions);
            Assert.Multiple(() =>
            {
                Assert.False(string.IsNullOrEmpty(permissions.Owner));
                Assert.NotEmpty(permissions.Rules);
            });
        }

        [Theory]
        [InlineData(0x10000000, "FullControl")]
        [InlineData(unchecked((int)0xA0000000), "ReadAndExecute")]
        [InlineData((int)FileSystemRights.FullControl, "FullControl")]
        [InlineData((int)(FileSystemRights.Modify | FileSystemRights.Synchronize), "Modify")]
        public void GetRights_Should_Name_The_Rights(int rights, string expected)
        {
            Assert.Equal(expected, FileSystemViewModelFactory.GetRights((FileSystemRights)rights));
        }

        [Theory]
        [InlineData("aurora.png", "ROR", true)]
        [InlineData("aurora.png", "*.PNG", true)]
        [InlineData("aurora.png", "*.jpg", false)]
        [InlineData("aurora.png", "aur", true)]
        [InlineData("aurora.png", "aur*", true)]
        [InlineData("aurora.png", "png*", false)]
        [InlineData("DSC_0412.jpg", "DSC_041?.jpg", true)]
        [InlineData("DSC_0412.jpg", "DSC_04?.jpg", false)]
        public void IsMatch_Should_Support_Wildcards(string name, string query, bool expected)
        {
            Assert.Equal(expected, FileSystemSearch.IsMatch(name, query));
        }

        [Fact]
        public async Task Search_Should_Include_Subfolders()
        {
            CreateFile("top.png", new byte[1]);
            CreateFile(Path.Combine("sub", "nested", "deep.png"), new byte[1]);
            CreateFile(Path.Combine("sub", "other.txt"), new byte[1]);
            var (viewModel, _) = await CreateViewModel();

            viewModel.SearchText = "*.png";
            await WaitFor(() => !viewModel.IsSearching);

            Assert.Multiple(() =>
            {
                Assert.Equal(["deep.png", "top.png"], viewModel.Entries!.Select(p => p.Name).Order());
                Assert.True(viewModel.IsSearchActive);
                Assert.StartsWith("2 matches", viewModel.SearchStatus);
            });
        }

        [Fact]
        public async Task Clearing_The_Search_Should_Restore_The_Folder_And_The_Selection()
        {
            CreateFile("a.png", new byte[1]);
            CreateFile("b.txt", new byte[1]);
            var (viewModel, root) = await CreateViewModel();
            viewModel.SelectedEntry = viewModel.Entries!.Single(p => p.Name == "a.png");

            viewModel.SelectedEntry = null;
            viewModel.SearchText = "b";
            await WaitFor(() => !viewModel.IsSearching);

            Assert.Equal("b.txt", Assert.Single(viewModel.Entries!).Name);

            viewModel.SearchText = string.Empty;

            Assert.Multiple(() =>
            {
                Assert.Same(root.Items, viewModel.Entries);
                Assert.Equal("a.png", viewModel.SelectedEntry?.Name);
            });
        }

        [Fact]
        public async Task Selecting_An_Entry_Should_Update_The_Inspector_And_Check_The_Header()
        {
            CreateFile("image.png", CreatePngBytes());
            var (viewModel, root) = await CreateViewModel();

            var file = Assert.IsType<ScarletFile>(viewModel.Entries!.Single());
            viewModel.SelectedEntry = file;
            await WaitFor(() => file.IsImage && viewModel.InspectedPermissions is not null);

            Assert.Multiple(() =>
            {
                Assert.Same(file, viewModel.Inspected);
                Assert.Same(file, viewModel.SelectedFile);
                Assert.Same(root, viewModel.SelectedContainer);
                Assert.Contains(viewModel.InspectedAttributes, p => p.Name == nameof(FileAttributes.Archive) && p.IsSet);
            });

            viewModel.SelectedEntry = null;

            Assert.Same(root, viewModel.Inspected);
        }

        [Fact]
        public async Task Selecting_A_Folder_Entry_Should_Not_Change_The_Container()
        {
            CreateFile(Path.Combine("sub", "a.txt"), new byte[1]);
            var (viewModel, root) = await CreateViewModel();

            viewModel.SelectedEntry = viewModel.Entries!.Single();

            Assert.Multiple(() =>
            {
                Assert.Same(root, viewModel.SelectedContainer);
                Assert.Equal("sub", viewModel.Inspected?.Name);
            });
        }

        [Fact]
        public async Task Items_Should_List_Folders_First()
        {
            CreateFile("a.txt", new byte[1]);
            CreateFile(Path.Combine("z", "b.txt"), new byte[1]);
            var (viewModel, _) = await CreateViewModel();

            Assert.Equal(["z", "a.txt"], viewModel.Entries!.Select(p => p.Name));
        }

        [Fact]
        public async Task Open_Should_Walk_The_Tree_To_A_Search_Result()
        {
            CreateFile(Path.Combine("sub", "nested", "a.txt"), new byte[1]);
            var options = new FileSystemOptionsViewModel([DriveType.Fixed], _fileAttributes, _folderAttributes);
            using var viewModel = new FileSystemViewModel(ImmediateScheduler.Instance, _factory, options);
            await viewModel.RefreshCommand.ExecuteAsync(null);

            var info = await _factory.GetDirectoryInfo(Path.Combine(_directory, "sub", "nested"), TestContext.Current.CancellationToken);
            using var result = new ScarletDirectory(ImmediateScheduler.Instance, info!, _fileAttributes, _folderAttributes, CreateParent(), _factory);

            await viewModel.OpenCommand.ExecuteAsync(result);
            await WaitFor(() => viewModel.SelectedContainer is not null);

            Assert.Multiple(() =>
            {
                Assert.NotSame(result, viewModel.SelectedContainer);
                Assert.Equal(result.FullName, viewModel.SelectedContainer?.FullName);
                Assert.Equal(viewModel.SelectedContainer?.Name, viewModel.Breadcrumbs[^1].Name);
            });
        }

        [Theory]
        [InlineData(@"C:\a\b\deep", @"C:\a", @"b\deep")]
        [InlineData(@"C:\a", @"C:\a", "a")]
        public void RelativeLocationConverter_Should_Return_The_Path_Below_The_Scope(string location, string scope, string expected)
        {
            var result = new RelativeLocationConverter().Convert([location, scope], typeof(string), null, CultureInfo.InvariantCulture);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToAttributeLetters_Should_Return_The_Set_Flags()
        {
            var result = new ToAttributeLetters().Convert(FileAttributes.ReadOnly | FileAttributes.Archive, null, null, null);

            Assert.Equal("RA", result);
        }

        [Fact]
        public async Task Selecting_Another_Container_Should_Clear_The_Search()
        {
            CreateFile("a.txt", new byte[1]);
            var (viewModel, root) = await CreateViewModel();
            viewModel.SearchText = "a";
            await WaitFor(() => !viewModel.IsSearching);

            viewModel.OnContainerSelected(root);

            Assert.Multiple(() =>
            {
                Assert.Empty(viewModel.SearchText);
                Assert.False(viewModel.IsSearchActive);
                Assert.Same(root.Items, viewModel.Entries);
            });
        }
    }
}
