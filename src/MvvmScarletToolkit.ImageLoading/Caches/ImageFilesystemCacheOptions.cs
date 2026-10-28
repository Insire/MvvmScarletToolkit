namespace MvvmScarletToolkit.ImageLoading.Caches
{
    public sealed class ImageFilesystemCacheOptions
    {
        public string CacheDirectoryPath { get; init; } = "cache/images/encoded/";

        public bool ClearCacheDirectoryOnInit { get; init; } = false;

        public bool CreateFolder { get; init; } = false;

        public bool IsEnabled { get; init; } = false;

        /// <summary>
        /// Files older than this are removed on startup. 0 disables the cleanup.
        /// </summary>
        public int MaxFileAgeInDays { get; init; } = 0;
    }
}
