namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageDataProviderOptions
    {
        /// <summary>
        /// Resources larger than this are rejected. 0 or less disables the limit.
        /// </summary>
        public long MaxDownloadSizeInBytes { get; init; } = 64L * 1024 * 1024;

        /// <summary>
        /// Upper bound for a complete web request including the download. <see cref="TimeSpan.Zero"/> or less disables the timeout.
        /// </summary>
        public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
    }
}
