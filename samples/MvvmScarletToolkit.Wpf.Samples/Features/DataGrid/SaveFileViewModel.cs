using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.ComponentModel;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    /// <summary>
    /// Selects where a file is saved: a folder from the file system browser and a file name. Selecting a file in the browser takes over its name.
    /// </summary>
    public sealed partial class SaveFileViewModel : ObservableObject, IDisposable
    {
        public FileSystemViewModel FileSystem { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilePath))]
        [NotifyPropertyChangedFor(nameof(CanSave))]
        public partial string FileName { get; set; }

        /// <summary>
        /// A rooted <see cref="FileName"/> replaces the selected folder
        /// </summary>
        public string FilePath => CanSave ? Path.Combine(Folder, FileName) : string.Empty;

        public bool CanSave => !string.IsNullOrWhiteSpace(FileName)
            && FileName.IndexOfAny(Path.GetInvalidPathChars()) < 0;

        private string Folder => FileSystem.SelectedContainer?.FullName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public SaveFileViewModel(FileSystemViewModel fileSystem, string fileName)
        {
            FileSystem = fileSystem;
            FileName = fileName;

            FileSystem.PropertyChanged += OnFileSystemChanged;
        }

        private void OnFileSystemChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FileSystemViewModel.SelectedContainer):
                    OnPropertyChanged(nameof(FilePath));
                    break;

                case nameof(FileSystemViewModel.SelectedFile) when FileSystem.SelectedFile is not null:
                    FileName = FileSystem.SelectedFile.Name;
                    break;
            }
        }

        public void Dispose()
        {
            FileSystem.PropertyChanged -= OnFileSystemChanged;
        }
    }
}
