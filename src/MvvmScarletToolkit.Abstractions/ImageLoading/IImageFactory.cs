using System.IO;

namespace MvvmScarletToolkit.Abstractions.ImageLoading
{
    public interface IImageFactory<T>
    {
        T From(Stream stream, ImageSize requestedSize);
    }
}
