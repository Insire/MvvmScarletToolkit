namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageServiceOptions
    {
        public uint DefaultHeight { get; init; } = 300;

        public uint DefaultWidth { get; init; } = 300;

        /// <summary>
        /// How long a failed load is remembered, so that the same broken resource is not requested again on every call.
        /// 0 or less disables this.
        /// </summary>
        public int FailedLoadRetryDelayInSeconds { get; init; } = 15;

        /// <summary>
        /// Disk cache writes happen in the background, after the image has been handed out.
        /// This limits how many run at once, so they do not compete with decoding images that are about to be shown.
        /// </summary>
        public int MaxConcurrentDiskCacheWrites { get; init; } = 2;
    }
}
