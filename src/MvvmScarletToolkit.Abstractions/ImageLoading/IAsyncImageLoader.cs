using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions.ImageLoading
{
    public interface IAsyncImageLoader<T>
        where T : class
    {
        public Task<T?> ProvideImageAsync(Uri? uri, ImageSize? requestedSize, Action<bool> requestedImageLoadsSlowly, CancellationToken cancellationToken = default);
    }
}
