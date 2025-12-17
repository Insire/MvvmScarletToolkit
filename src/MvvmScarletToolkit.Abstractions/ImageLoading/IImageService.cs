namespace MvvmScarletToolkit
{
    public interface IImageService<T>
        where T : class
    {
        Task<T?> ProvideImageAsync(Uri? uri, ImageSize? requestedSize, Func<bool, Task> requestedImageLoadsSlowly, CancellationToken cancellationToken = default);
    }
}
