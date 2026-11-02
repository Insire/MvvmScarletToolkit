namespace MvvmScarletToolkit.ImageLoading.Caches.Data
{
    public sealed class ImageDataFileystemCacheOptions
    {
        public string CacheDirectoryPath { get; init; } = "cache/images/raw/";

        public bool ClearCacheDirectoryOnInit { get; init; } = false;

        public bool CreateFolder { get; init; } = false;

        public bool IsEnabled { get; init; } = false;

        /// <summary>
        /// Files older than this are removed on startup. 0 disables the cleanup.
        /// </summary>
        public int MaxFileAgeInDays { get; init; } = 0;
    }
}
