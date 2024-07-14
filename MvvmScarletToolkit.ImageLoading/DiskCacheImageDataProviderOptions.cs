namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class DiskCacheImageDataProviderOptions
    {
        public string CacheDirectoryPath { get; init; } = "Cache/Images/";

        public bool CreateFolder { get; init; } = false;

        public bool IsEnabled { get; init; } = false;
    }
}
