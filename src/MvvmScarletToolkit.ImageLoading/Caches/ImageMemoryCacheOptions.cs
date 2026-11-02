namespace MvvmScarletToolkit.ImageLoading.Caches
{
    public sealed class ImageMemoryCacheOptions
    {
        public bool IsEnabled { get; init; } = false;

        public int AbsoluteExpirationOffsetInSeconds { get; init; } = 35;

        /// <summary>
        /// How many decoded images are kept in memory at most. Above that, the least recently used ones are removed.
        /// Only the entries of this cache are counted, even when the <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> is shared.
        /// 0 or less disables this.
        /// </summary>
        public int MaxEntries { get; init; } = 100;
    }
}
