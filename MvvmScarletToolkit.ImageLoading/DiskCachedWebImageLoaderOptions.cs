namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class DiskCachedWebImageLoaderOptions : BaseWebImageLoaderOptions
    {
        public string CacheFolder { get; init; } = "Cache/Images/";

        public bool CreateFolder { get; init; } = false;
    }
}
