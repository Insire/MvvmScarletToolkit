using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit

{
    /// <summary>
    /// Fetches the resource behind an uri and returns it as <see cref="Stream"/>
    /// </summary>
    public interface IImageDataProvider
    {
        Task<Stream?> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
