namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class MemoryCacheImageProviderOptions
    {
        public bool IsEnabled { get; init; } = false;

        public int AbsoluteExpirationOffsetInSeconds { get; init; } = 35;
    }
}
