using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions.ImageLoading
{
    public interface IImageDataProvider
    {
        Task<Stream?> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
