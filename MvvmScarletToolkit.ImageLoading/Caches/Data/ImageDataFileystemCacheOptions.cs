namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageDataFileystemCacheOptions
    {
        public string CacheDirectoryPath { get; init; } = "cache/images/raw/";

        public bool CreateFolder { get; init; } = false;

        public bool IsEnabled { get; init; } = false;
    }
}
