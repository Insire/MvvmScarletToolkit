using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;
using System.Reactive.Concurrency;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Asks for a file or folder in a modal <see cref="FileSystemDialogWindow"/>. Can be used from any thread.
    /// Requires the styles of Generic.xaml in the application resources.
    /// </summary>
    public sealed class FileSystemDialog
    {
        private readonly IScheduler _scheduler;
        private readonly IFileSystemViewModelFactory _factory;
        private readonly FileSystemOptionsViewModel _options;
        private readonly IScarletDispatcher _dispatcher;

        public FileSystemDialog(IScheduler scheduler, IFileSystemViewModelFactory factory, FileSystemOptionsViewModel options, IScarletDispatcher dispatcher)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// Asks for an existing file.
        /// </summary>
        /// <param name="initialFolder">the folder to start in, the documents folder by default</param>
        /// <param name="token">closes the dialog when canceled</param>
        /// <param name="filters">the files the user can choose from, the first one is applied initially. All files by default</param>
        /// <returns>the selected path, null when the user canceled</returns>
        public Task<string?> PickFile(string? initialFolder, CancellationToken token, params FileSystemFilter[] filters)
        {
            return Show(_options, fileSystem => new FilePickerViewModel(fileSystem, filters), initialFolder, token);
        }

        /// <summary>
        /// Asks where to save a file: a folder and a file name. Asks before an existing file is replaced.
        /// </summary>
        /// <param name="initialFolder">the folder to start in, the documents folder by default</param>
        /// <param name="fileName">the suggested file name</param>
        /// <param name="token">closes the dialog when canceled</param>
        /// <param name="filters">the files shown in the folder, the first one is applied initially. A name without an extension gets the one of the selected filter. All files by default</param>
        /// <returns>the path to save to, null when the user canceled</returns>
        public Task<string?> PickSaveFile(string? initialFolder, string fileName, CancellationToken token, params FileSystemFilter[] filters)
        {
            return Show(_options, fileSystem => new SaveFilePickerViewModel(fileSystem, fileName, filters), initialFolder, token);
        }

        /// <summary>
        /// Asks for an existing folder.
        /// </summary>
        /// <param name="initialFolder">the folder to start in, the documents folder by default</param>
        /// <param name="token">closes the dialog when canceled</param>
        /// <returns>the selected path, null when the user canceled</returns>
        public Task<string?> PickFolder(string? initialFolder, CancellationToken token)
        {
            // without file attributes no file matches, so the browser only lists folders
            var options = new FileSystemOptionsViewModel(_options.DriveTypes, [], _options.FolderAttributes);

            return Show(options, CreateFolderPicker, initialFolder, token);
        }

        private static FolderPickerViewModel CreateFolderPicker(FileSystemViewModel fileSystem)
        {
            fileSystem.IsPreviewVisible = false;

            return new FolderPickerViewModel(fileSystem);
        }

        private Task<string?> Show(FileSystemOptionsViewModel options, Func<FileSystemViewModel, FileSystemPickerViewModel> createPicker, string? initialFolder, CancellationToken token)
        {
            return _dispatcher.Invoke(() =>
            {
                using var fileSystem = new FileSystemViewModel(_scheduler, _factory, options)
                {
                    InitialFolder = initialFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                };
                using var picker = createPicker(fileSystem);

                // the new window registers itself in Application.Windows, so the owner has to be determined first
                var owner = GetOwner();
                var window = new FileSystemDialogWindow(picker)
                {
                    Owner = owner,
                };

                if (picker is SaveFilePickerViewModel savePicker)
                {
                    savePicker.ConfirmOverwrite ??= path => ConfirmOverwrite(window, path);
                }

                using var registration = token.Register(() => window.Dispatcher.InvokeAsync(window.Close));

                return window.ShowDialog() == true ? picker.SelectedPath : null;
            }, token);
        }

        private static bool ConfirmOverwrite(Window window, string path)
        {
            var message = $"{Path.GetFileName(path)} already exists.{Environment.NewLine}Do you want to replace it?";

            return MessageBox.Show(window, message, window.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        private static Window? GetOwner()
        {
            var application = Application.Current;

            return application?.Windows.OfType<Window>().FirstOrDefault(p => p.IsActive) ?? application?.MainWindow;
        }
    }
}
