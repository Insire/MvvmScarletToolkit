using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IImageDataCache
    {
        Task<Stream?> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);

        Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default);
    }
}
