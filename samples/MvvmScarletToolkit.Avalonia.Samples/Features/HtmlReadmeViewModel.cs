using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.IO;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    /// <summary>
    /// The repository readme as html, rendered by an HtmlTextBlock while the user edits it.
    /// </summary>
    public sealed partial class HtmlReadmeViewModel : ViewModelBase
    {
        private const string ResourceName = "MvvmScarletToolkit.Avalonia.Samples.Resources.readme.html";

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

        /// <summary>
        /// Text to show in bold in the preview.
        /// </summary>
        [ObservableProperty]
        public partial string? Highlight { get; set; }

        public bool IsModified => !string.Equals(Html, OriginalHtml, StringComparison.Ordinal);

        public ConcurrentCommandBase RestoreCommand { get; }

        public HtmlReadmeViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            OriginalHtml = LoadReadme();
            Html = OriginalHtml;

            RestoreCommand = CommandBuilder
                .Create(Restore, () => IsModified)
                .WithDisabledReason(() => "The html is unchanged")
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

            // shared with the WPF sample, which loads its images from pack uris.
            // HtmlTextBlock keeps line breaks as written, the file may have been checked out with windows line endings
            return reader.ReadToEnd()
                .Replace("pack://application:,,,/Resources/", "avares://MvvmScarletToolkit.Avalonia.Samples/Assets/")
                .Replace("\r\n", "\n")
                .Trim();
        }

        private Task Restore()
        {
            Html = OriginalHtml;

            return Task.CompletedTask;
        }
    }
}
