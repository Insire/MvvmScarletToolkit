using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IImageMemoryCache<TImage>
        where TImage : class
    {
        Task CacheImageAsync(TImage image, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);

        Task<TImage?> GetImageAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);
    }
}
