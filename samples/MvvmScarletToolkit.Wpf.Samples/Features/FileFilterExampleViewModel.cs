using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    /// <summary>
    /// A file dialog using a specific set of filters, and the file that was picked with it.
    /// </summary>
    public sealed partial class FileFilterExampleViewModel : ObservableObject
    {
        private readonly FileSystemDialog _dialog;
        private readonly Func<FileSystemFilter[]> _getFilters;

        public string Name { get; }

        public string Description { get; }

        [ObservableProperty]
        public partial string? File { get; private set; }

        /// <param name="getFilters">evaluated for every dialog, so that filters can depend on user input</param>
        public FileFilterExampleViewModel(FileSystemDialog dialog, string name, string description, Func<FileSystemFilter[]> getFilters)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _getFilters = getFilters ?? throw new ArgumentNullException(nameof(getFilters));

            Name = name;
            Description = description;
        }

        [RelayCommand]
        private async Task Pick(CancellationToken token)
        {
            // canceling the dialog keeps the previous result
            File = await _dialog.PickFile(Path.GetDirectoryName(File), token, _getFilters()) ?? File;
        }
    }
}
