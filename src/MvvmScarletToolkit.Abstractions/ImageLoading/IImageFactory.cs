using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Turns a stream into a GUI-framework raw-image representation.
    /// </summary>
    /// <typeparam name="TImage"></typeparam>
    public interface IImageFactory<TImage>
        where TImage : class
    {
        Task<TImage> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default);

        Task ToAsync(Stream stream, TImage image, CancellationToken cancellationToken = default);
    }
}
