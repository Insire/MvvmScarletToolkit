namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    /// <summary>
    /// The result of reading the header of a file.
    /// </summary>
    /// <param name="MimeType">The type the header identifies, <see langword="null"/> when no signature matches.</param>
    /// <param name="IsReadable"><see langword="false"/> when the file could not be opened.</param>
    public sealed record ScarletFileHeader(string? MimeType, bool IsReadable);
}
