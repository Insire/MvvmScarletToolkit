namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ImageFilesystemCacheOptions
    {
        public string CacheDirectoryPath { get; init; } = "cache/images/encoded/";

        public bool CreateFolder { get; init; } = false;

        public bool IsEnabled { get; init; } = false;
    }
}
