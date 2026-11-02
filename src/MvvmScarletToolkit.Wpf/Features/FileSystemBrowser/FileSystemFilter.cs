namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Restricts the displayed files to the ones matching any of the <see cref="Patterns"/>, folders stay displayed.
    /// </summary>
    public sealed class FileSystemFilter
    {
        public static FileSystemFilter AllFiles { get; } = new FileSystemFilter("All files", "*");

        public string Name { get; }

        /// <summary>
        /// Evaluated by <see cref="FileSystemSearch.IsMatch"/>, for example *.png
        /// </summary>
        public IReadOnlyList<string> Patterns { get; }

        public FileSystemFilter(string name, params string[] patterns)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(patterns);

            if (patterns.Length == 0)
            {
                throw new ArgumentException("A filter requires at least one pattern.", nameof(patterns));
            }

            Name = name;
            Patterns = patterns;
        }

        public bool IsMatch(string fileName)
        {
            return Patterns.Any(p => FileSystemSearch.IsMatch(fileName, p));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
