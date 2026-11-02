namespace MvvmScarletToolkit
{
    /// <summary>
    /// Turns a stream into a GUI-framework raw-image representation.
    /// </summary>
    /// <remarks>
    /// Methods are called from background threads, images have to be usable from any thread (e.g. frozen in WPF).
    /// </remarks>
    /// <typeparam name="TImage"></typeparam>
    public interface IImageFactory<TImage>
        where TImage : class
    {
        /// <remarks>
        /// <paramref name="stream"/> stays owned by the caller, it must not be disposed or kept.
        /// </remarks>
        Task<TImage> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default);

        /// <remarks>
        /// <paramref name="stream"/> stays owned by the caller, it must not be disposed or kept.
        /// Has to throw when nothing could be written, so that no incomplete data gets cached.
        /// </remarks>
        Task ToAsync(Stream stream, TImage image, CancellationToken cancellationToken = default);
    }
}
