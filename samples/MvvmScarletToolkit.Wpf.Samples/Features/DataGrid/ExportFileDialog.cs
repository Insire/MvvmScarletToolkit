using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Samples.Controls;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    /// <summary>
    /// Asks for the file to export to. Can be used from any thread.
    /// </summary>
    public sealed class ExportFileDialog
    {
        private readonly FileSystemViewModel _fileSystem;
        private readonly IScarletDispatcher _dispatcher;

        public ExportFileDialog(FileSystemViewModel fileSystem, IScarletDispatcher dispatcher)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <returns>the selected path, null when the user canceled</returns>
        public Task<string?> Show(string fileName, CancellationToken token)
        {
            return _dispatcher.Invoke(() =>
            {
                using var viewModel = new SaveFileViewModel(_fileSystem, fileName);

                var window = new SaveFileWindow
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                };

                return window.ShowDialog() == true ? viewModel.FilePath : null;
            }, token);
        }
    }
}
