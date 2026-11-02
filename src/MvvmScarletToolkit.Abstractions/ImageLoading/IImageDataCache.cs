namespace MvvmScarletToolkit
{
    public interface IImageDataCache
    {
        /// <returns>
        /// A readable stream positioned at its start, or <see cref="Stream.Null"/> when nothing is cached.
        /// The caller owns the stream and has to dispose it.
        /// </returns>
        Task<Stream> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Caches the content of <paramref name="imageDataStream"/>, starting from its beginning when it is seekable.
        /// </summary>
        /// <remarks>
        /// The stream stays owned by the caller. It is only read while the returned task is running and not kept afterwards.
        /// </remarks>
        Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);
    }
}
