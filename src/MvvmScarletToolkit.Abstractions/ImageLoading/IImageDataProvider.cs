namespace MvvmScarletToolkit

{
    /// <summary>
    /// Fetches the resource behind an uri and returns it as <see cref="Stream"/>
    /// </summary>
    public interface IImageDataProvider
    {
        /// <returns>
        /// A readable stream positioned at its start, or <see cref="Stream.Null"/> when the resource could not be fetched
        /// or <paramref name="cancellationToken"/> was cancelled. The caller owns the stream and has to dispose it.
        /// </returns>
        Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
