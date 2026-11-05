using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Opens urls and files with their default application.
    /// </summary>
    internal static class ShellExecute
    {
        /// <summary>
        /// shell execute instead of "cmd /c start", cmd would interpret characters like &amp; in the target,
        /// cutting off query strings or running whatever follows as another command
        /// </summary>
        public static ProcessStartInfo CreateStartInfo(string target)
        {
            return new ProcessStartInfo(target)
            {
                UseShellExecute = true,
            };
        }

        public static ProcessStartInfo CreateStartInfo(Uri uri)
        {
            return CreateStartInfo(uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString);
        }

        public static void Start(ProcessStartInfo info)
        {
            using (Process.Start(info))
            { }
        }

        /// <summary>
        /// Whether <paramref name="uri"/> is a web or mail link. Other schemes like file: could start any program.
        /// </summary>
        public static bool IsWebLink(Uri? uri)
        {
            return uri is { IsAbsoluteUri: true }
                && (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals(Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase));
        }
    }
}
