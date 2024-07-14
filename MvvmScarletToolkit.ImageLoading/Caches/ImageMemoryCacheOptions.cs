namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageMemoryCacheOptions
    {
        public bool IsEnabled { get; init; } = false;

        public int AbsoluteExpirationOffsetInSeconds { get; init; } = 35;
    }
}
