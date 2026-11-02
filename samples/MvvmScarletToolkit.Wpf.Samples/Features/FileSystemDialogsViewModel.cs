using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class FileSystemDialogsViewModel : ObservableObject
    {
        private static readonly FileSystemFilter _images = new FileSystemFilter("Images", "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp");
        private static readonly FileSystemFilter _documents = new FileSystemFilter("Documents", "*.pdf", "*.docx", "*.txt", "*.md");
        private static readonly FileSystemFilter _settings = new FileSystemFilter("Settings", "*.ini", "*.json", "*.xml", "*.config");

        private readonly FileSystemDialog _dialog;

        public IReadOnlyList<FileFilterExampleViewModel> Examples { get; }

        public FileFilterExampleViewModel CustomExample { get; }

        /// <summary>
        /// The patterns of the <see cref="CustomExample"/>, separated by semicolons.
        /// </summary>
        [ObservableProperty]
        public partial string CustomPatterns { get; set; } = "*.cs;*.xaml";

        [ObservableProperty]
        public partial string? Folder { get; private set; }

        public FileSystemDialogsViewModel(FileSystemDialog dialog)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));

            Examples =
            [
                new FileFilterExampleViewModel(dialog, "No filter", "Every file can be selected.", () => []),
                new FileFilterExampleViewModel(dialog, "Single filter", "Only images can be selected, the user can't change that.", () => [_images]),
                new FileFilterExampleViewModel(dialog, "Several filters", "The user switches between images, documents and settings. Other files can't be selected.", () => [_images, _documents, _settings]),
                new FileFilterExampleViewModel(dialog, "Preferred filter", "Starts with documents, the user can fall back to all files.", () => [_documents, FileSystemFilter.AllFiles]),
                new FileFilterExampleViewModel(dialog, "File names", "Patterns aren't limited to extensions: names starting with a dot or containing \"readme\".", () => [new FileSystemFilter("Dot and readme files", ".*", "readme")]),
            ];

            CustomExample = new FileFilterExampleViewModel(dialog, "Custom patterns", "Uses the patterns entered here, without any it behaves like no filter.", GetCustomFilters);
        }

        private FileSystemFilter[] GetCustomFilters()
        {
            var patterns = CustomPatterns.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return patterns.Length == 0 ? [] : [new FileSystemFilter(string.Join(", ", patterns), patterns)];
        }

        [RelayCommand]
        private async Task PickFolder(CancellationToken token)
        {
            Folder = await _dialog.PickFolder(Folder, token) ?? Folder;
        }
    }
}
