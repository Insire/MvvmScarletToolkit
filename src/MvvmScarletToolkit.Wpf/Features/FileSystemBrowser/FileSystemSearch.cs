using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO.Enumeration;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// The matches and skipped folders of a single searched directory.
    /// </summary>
    public sealed record FileSystemSearchBatch(IReadOnlyList<IFileSystemChild> Items, bool IsSkipped);

    public static class FileSystemSearch
    {
        /// <summary>
        /// A query containing * or ? has to match the whole name, any other query matches part of the name.
        /// </summary>
        public static bool IsMatch(string name, string query)
        {
            return query.AsSpan().IndexOfAny('*', '?') >= 0
                ? FileSystemName.MatchesSimpleExpression(query, name, ignoreCase: true)
                : name.Contains(query, StringComparison.OrdinalIgnoreCase);
        }
    }
}
