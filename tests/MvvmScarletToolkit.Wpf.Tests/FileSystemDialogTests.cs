using Microsoft.Extensions.Logging.Abstractions;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.IO;
using System.Reactive.Concurrency;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class FileSystemDialogTests : TraceTestBase, IDisposable
    {
        private static readonly FileAttributes[] _fileAttributes = [FileAttributes.Archive, FileAttributes.Normal];
        private static readonly FileAttributes[] _folderAttributes = [FileAttributes.Directory];

        // windows on other test threads must not run the steps of this test
        [ThreadStatic]
        private static Func<FileSystemDialogWindow, FileSystemPickerViewModel, Task>? _whileShown;

        [ThreadStatic]
        private static Exception? _failure;

        private readonly string _directory;
        private readonly FileSystemViewModelFactory _factory;

        static FileSystemDialogTests()
        {
            // ShowDialog blocks the test and there is no Application to look up the window
            EventManager.RegisterClassHandler(typeof(FileSystemDialogWindow), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
        }

        public FileSystemDialogTests()
        {
            _directory = Path.Combine(Path.GetTempPath(), nameof(FileSystemDialogTests), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);

            _factory = new FileSystemViewModelFactory(NullLogger<FileSystemViewModelFactory>.Instance, ImmediateScheduler.Instance);
        }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }

        private static async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var window = (FileSystemDialogWindow)sender;

            try
            {
                await _whileShown!(window, (FileSystemPickerViewModel)window.DataContext);
            }
            catch (Exception ex)
            {
                _failure = ex;
                window.Close();
            }
        }

        private static async Task<string?> Show(Func<Task<string?>> show, Func<FileSystemDialogWindow, FileSystemPickerViewModel, Task> whileShown)
        {
            _failure = null;
            _whileShown = whileShown;

            var result = await show();

            return _failure is null ? result : throw new InvalidOperationException("The steps failed while the dialog was open.", _failure);
        }

        /// <summary>
        /// The default template refreshes when it loads, the tests run without the styles of the application.
        /// </summary>
        private async Task Load(FileSystemPickerViewModel picker)
        {
            await picker.FileSystem.RefreshCommand.ExecuteAsync(null);
            await Ui.WaitUntil(() => string.Equals(picker.FileSystem.SelectedContainer?.FullName, _directory, StringComparison.OrdinalIgnoreCase));
        }

        private FileSystemDialog CreateDialog()
        {
            var options = new FileSystemOptionsViewModel([DriveType.Fixed], _fileAttributes, _folderAttributes);

            return new FileSystemDialog(ImmediateScheduler.Instance, _factory, options, new TestDispatcher());
        }

        private string CreateFile(string name)
        {
            var path = Path.Combine(_directory, name);
            File.WriteAllBytes(path, new byte[1]);

            return path;
        }

        [Fact]
        public Task PickFolder_Should_Return_The_Confirmed_Folder()
        {
            return StaRunner.Run(async () =>
            {
                var result = await Show(() => CreateDialog().PickFolder(_directory, CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);

                    picker.ConfirmCommand.Execute(null);
                });

                Assert.Equal(_directory, result, ignoreCase: true);
            });
        }

        [Fact]
        public Task PickFolder_Should_Only_List_Folders()
        {
            return StaRunner.Run(async () =>
            {
                CreateFile("a.txt");
                Directory.CreateDirectory(Path.Combine(_directory, "sub"));

                await Show(() => CreateDialog().PickFolder(_directory, CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => picker.FileSystem.Entries?.Count > 0);

                    Assert.Multiple(() =>
                    {
                        Assert.Equal("sub", Assert.Single(picker.FileSystem.Entries!).Name);
                        Assert.False(picker.FileSystem.IsPreviewVisible);
                        Assert.Empty(picker.Filters);
                        Assert.Equal(picker.Title, window.Title);
                    });

                    window.Close();
                });
            });
        }

        [Fact]
        public Task PickFile_Should_Return_The_Opened_File()
        {
            return StaRunner.Run(async () =>
            {
                var path = CreateFile("a.txt");

                var result = await Show(() => CreateDialog().PickFile(_directory, CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => picker.FileSystem.Entries?.Count > 0);

                    picker.FileSystem.SelectedEntry = picker.FileSystem.Entries!.Single();
                    await picker.FileSystem.OpenCommand.ExecuteAsync(picker.FileSystem.SelectedEntry);
                });

                Assert.Equal(path, result, ignoreCase: true);
            });
        }

        [Fact]
        public Task PickFile_Should_Apply_The_First_Filter()
        {
            return StaRunner.Run(async () =>
            {
                CreateFile("a.txt");
                CreateFile("b.png");
                var images = new FileSystemFilter("Images", "*.png");
                var all = FileSystemFilter.AllFiles;

                await Show(() => CreateDialog().PickFile(_directory, CancellationToken.None, images, all), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => picker.FileSystem.Entries?.Count > 0);

                    Assert.Multiple(() =>
                    {
                        Assert.Equal([images, all], picker.Filters);
                        Assert.Same(images, picker.SelectedFilter);
                        Assert.Equal("b.png", Assert.Single(picker.FileSystem.Entries!).Name);
                    });

                    window.Close();
                });
            });
        }

        [Fact]
        public Task Should_Start_In_The_Documents_Folder_By_Default()
        {
            return StaRunner.Run(async () =>
            {
                string? initialFolder = null;

                await Show(() => CreateDialog().PickFile(null, CancellationToken.None), (window, picker) =>
                {
                    initialFolder = picker.FileSystem.InitialFolder;
                    window.Close();

                    return Task.CompletedTask;
                });

                Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), initialFolder);
            });
        }

        [Fact]
        public Task Should_Return_Null_When_Closed()
        {
            return StaRunner.Run(async () =>
            {
                var path = CreateFile("a.txt");

                var result = await Show(() => CreateDialog().PickFile(_directory, CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => picker.FileSystem.Entries?.Count > 0);

                    // a selection doesn't count without confirming it
                    picker.FileSystem.SelectedEntry = picker.FileSystem.Entries!.Single();

                    window.Close();
                });

                Assert.Null(result);
            });
        }

        [Fact]
        public Task Should_Close_And_Return_Null_When_The_Token_Is_Canceled()
        {
            return StaRunner.Run(async () =>
            {
                using var cancellation = new CancellationTokenSource();

                var result = await Show(() => CreateDialog().PickFolder(_directory, cancellation.Token), (window, picker) => cancellation.CancelAsync());

                Assert.Null(result);
            });
        }

        [Fact]
        public void Should_Require_Its_Dependencies()
        {
            var options = new FileSystemOptionsViewModel([], [], []);
            var dispatcher = new TestDispatcher();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new FileSystemDialog(null!, _factory, options, dispatcher));
                Assert.Throws<ArgumentNullException>(() => new FileSystemDialog(ImmediateScheduler.Instance, null!, options, dispatcher));
                Assert.Throws<ArgumentNullException>(() => new FileSystemDialog(ImmediateScheduler.Instance, _factory, null!, dispatcher));
                Assert.Throws<ArgumentNullException>(() => new FileSystemDialog(ImmediateScheduler.Instance, _factory, options, null!));
            });
        }
    }
}
