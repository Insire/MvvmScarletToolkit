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

        private static readonly FileSystemFilter _html = new("Html", "*.html");

        [Fact]
        public Task PickSaveFile_Should_Return_The_Folder_And_Name_With_The_Filter_Extension()
        {
            return StaRunner.Run(async () =>
            {
                var result = await Show(() => CreateDialog().PickSaveFile(_directory, "readme", CancellationToken.None, _html, FileSystemFilter.AllFiles), async (window, picker) =>
                {
                    await Load(picker);

                    var save = Assert.IsType<SaveFilePickerViewModel>(picker);
                    Assert.Multiple(() =>
                    {
                        Assert.Equal("Save", save.ConfirmText);
                        Assert.Equal(Path.Combine(_directory, "readme.html"), save.SelectedPath, ignoreCase: true);
                    });

                    picker.ConfirmCommand.Execute(null);
                });

                Assert.Equal(Path.Combine(_directory, "readme.html"), result, ignoreCase: true);
            });
        }

        [Fact]
        public Task PickSaveFile_Should_Show_A_Focused_File_Name_Box()
        {
            return StaRunner.Run(async () =>
            {
                await Show(() => CreateDialog().PickSaveFile(_directory, "readme.html", CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => FindTextBox(window, "FileName") is not null);

                    var textBox = FindTextBox(window, "FileName")!;
                    Assert.Multiple(() =>
                    {
                        Assert.Equal("readme.html", textBox.Text);
                        Assert.True(textBox.IsKeyboardFocusWithin);

                        // typing replaces the name, the extension stays
                        Assert.Equal("readme", textBox.SelectedText);
                    });

                    textBox.Text = "other.html";
                    Assert.Equal("other.html", ((SaveFilePickerViewModel)picker).FileName);

                    window.Close();
                });
            });
        }

        [Fact]
        public Task PickSaveFile_Should_Take_The_Name_Of_A_Selected_File()
        {
            return StaRunner.Run(async () =>
            {
                CreateFile("existing.html");

                await Show(() => CreateDialog().PickSaveFile(_directory, "readme.html", CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);
                    await Ui.WaitUntil(() => picker.FileSystem.Entries?.Count > 0);

                    picker.FileSystem.SelectedEntry = picker.FileSystem.Entries!.Single();

                    Assert.Equal("existing.html", ((SaveFilePickerViewModel)picker).FileName);

                    window.Close();
                });
            });
        }

        [Fact]
        public Task PickSaveFile_Should_Ask_Before_Replacing_A_File()
        {
            return StaRunner.Run(async () =>
            {
                var existing = CreateFile("readme.html");
                var asked = new List<string>();

                var result = await Show(() => CreateDialog().PickSaveFile(_directory, "readme.html", CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);

                    var save = (SaveFilePickerViewModel)picker;

                    // declining keeps the dialog open
                    save.ConfirmOverwrite = path =>
                    {
                        asked.Add(path);
                        return false;
                    };
                    picker.ConfirmCommand.Execute(null);

                    Assert.True(window.IsVisible);

                    save.ConfirmOverwrite = path =>
                    {
                        asked.Add(path);
                        return true;
                    };
                    picker.ConfirmCommand.Execute(null);
                });

                Assert.Equal(existing, result, ignoreCase: true);
                Assert.Equal(2, asked.Count);
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("a/b.html")]
        [InlineData("a:b.html")]
        public Task PickSaveFile_Should_Not_Confirm_Invalid_Names(string fileName)
        {
            return StaRunner.Run(async () =>
            {
                await Show(() => CreateDialog().PickSaveFile(_directory, fileName, CancellationToken.None), async (window, picker) =>
                {
                    await Load(picker);

                    Assert.Multiple(() =>
                    {
                        Assert.Null(picker.SelectedPath);
                        Assert.False(picker.ConfirmCommand.CanExecute(null));
                    });

                    window.Close();
                });
            });
        }

        [Theory]
        [InlineData("readme", "*.html", "readme.html")]
        [InlineData("readme.txt", "*.html", "readme.txt")]
        [InlineData("readme", "*", "readme")]
        [InlineData("readme", "*.htm*", "readme")]
        [InlineData("readme", null, "readme")]
        public void WithDefaultExtension_Should_Only_Use_Simple_Patterns(string fileName, string? pattern, string expected)
        {
            var filter = pattern is null ? null : new FileSystemFilter("filter", pattern);

            Assert.Equal(expected, SaveFilePickerViewModel.WithDefaultExtension(fileName, filter));
        }

        private static System.Windows.Controls.TextBox? FindTextBox(DependencyObject parent, string automationId)
        {
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is System.Windows.Controls.TextBox textBox && System.Windows.Automation.AutomationProperties.GetAutomationId(textBox) == automationId)
                {
                    return textBox;
                }

                if (FindTextBox(child, automationId) is { } found)
                {
                    return found;
                }
            }

            return null;
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
