using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    /// <summary>
    /// The repository readme as html, rendered by an HtmlTextBlock.
    /// </summary>
    public sealed class HtmlReadmeViewModel
    {
        private const string ResourceName = "MvvmScarletToolkit.Wpf.Samples.Resources.readme.html";

        public string Html { get; }

        public HtmlReadmeViewModel()
        {
            using var stream = typeof(HtmlReadmeViewModel).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"The embedded resource {ResourceName} is missing.");
            using var reader = new StreamReader(stream);

            // HtmlTextBlock keeps line breaks as written, the file may have been checked out with windows line endings
            Html = reader.ReadToEnd().Replace("\r\n", "\n").Trim();
        }
    }
}
