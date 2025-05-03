using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IImageService<T>
        where T : class
    {
        public Task<T?> ProvideImageAsync(Uri? uri, ImageSize? requestedSize, Action<bool> requestedImageLoadsSlowly, CancellationToken cancellationToken = default);
    }
}
