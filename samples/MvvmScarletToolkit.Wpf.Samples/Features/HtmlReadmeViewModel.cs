using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    /// <summary>
    /// The repository readme as html, rendered by an HtmlTextBlock while the user edits it.
    /// </summary>
    public sealed partial class HtmlReadmeViewModel : ViewModelBase
    {
        private const string ResourceName = "MvvmScarletToolkit.Wpf.Samples.Resources.readme.html";
        private const string DefaultFileName = "readme.html";

        private static readonly FileSystemFilter _htmlFiles = new FileSystemFilter("Html files", "*.html", "*.htm");

        private readonly FileSystemDialog _dialog;

        /// <summary>
        /// The embedded readme, restored by <see cref="RestoreCommand"/>.
        /// </summary>
        public string OriginalHtml { get; }

        /// <summary>
        /// The edited html, the preview follows it.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsModified))]
        public partial string Html { get; set; }

        public bool IsModified => !string.Equals(Html, OriginalHtml, StringComparison.Ordinal);

        /// <summary>
        /// The opened file, relative image sources and links in it are resolved against it. The embedded readme only uses absolute ones.
        /// </summary>
        [ObservableProperty]
        public partial Uri? BaseUri { get; private set; }

        /// <summary>
        /// Where the html was saved last, or why saving failed.
        /// </summary>
        [ObservableProperty]
        public partial string? Status { get; private set; }

        public ConcurrentCommandBase OpenCommand { get; }

        public ConcurrentCommandBase RestoreCommand { get; }

        public ConcurrentCommandBase SaveCommand { get; }

        private string? _lastFolder;

        public HtmlReadmeViewModel(IScarletCommandBuilder commandBuilder, FileSystemDialog dialog)
            : base(commandBuilder)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));

            OriginalHtml = LoadReadme();
            Html = OriginalHtml;

            OpenCommand = CommandBuilder
                .Create(Open)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution(nameof(OpenCommand), "Opening...")
                .Build();

            RestoreCommand = CommandBuilder
                .Create(Restore, () => IsModified)
                .WithDisabledReason(() => "The html is unchanged")
                .Build();

            SaveCommand = CommandBuilder
                .Create(Save)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution(nameof(SaveCommand), "Saving...")
                .Build();
        }

        partial void OnHtmlChanged(string value)
        {
            // the editor updates Html after a delay, so the requery of the last keystroke already happened
            CommandManager.InvalidateRequerySuggested();
        }

        private static string LoadReadme()
        {
            using var stream = typeof(HtmlReadmeViewModel).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"The embedded resource {ResourceName} is missing.");
            using var reader = new StreamReader(stream);

            // HtmlTextBlock keeps line breaks as written, the file may have been checked out with windows line endings
            return reader.ReadToEnd().Replace("\r\n", "\n").Trim();
        }

        private Task Restore()
        {
            BaseUri = null;
            Html = OriginalHtml;
            Status = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Replaces the edited html with the content of a file. <see cref="RestoreCommand"/> still goes back to the embedded readme.
        /// </summary>
        private async Task Open(CancellationToken token)
        {
            var path = await _dialog.PickFile(_lastFolder, token, _htmlFiles, FileSystemFilter.AllFiles);
            if (path is null)
            {
                return;
            }

            try
            {
                var html = await File.ReadAllTextAsync(path, token);

                // set first, so the new html is rendered once with the right base
                BaseUri = new Uri(path);

                // same as the embedded readme, HtmlTextBlock keeps line breaks as written
                Html = html.Replace("\r\n", "\n");

                _lastFolder = Path.GetDirectoryName(path);
                Status = $"Opened {path}";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Status = $"Opening failed: {ex.Message}";
            }
        }

        private async Task Save(CancellationToken token)
        {
            var path = await _dialog.PickSaveFile(_lastFolder, DefaultFileName, token, _htmlFiles, FileSystemFilter.AllFiles);
            if (path is null)
            {
                return;
            }

            try
            {
                await File.WriteAllTextAsync(path, Html, token);

                _lastFolder = Path.GetDirectoryName(path);
                Status = $"Saved to {path}";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Status = $"Saving failed: {ex.Message}";
            }
        }
    }
}
