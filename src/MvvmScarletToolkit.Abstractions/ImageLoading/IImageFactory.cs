using System.IO;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Turns a stream into a GUI-framework raw-image representation.
    /// </summary>
    /// <typeparam name="TImage"></typeparam>
    public interface IImageFactory<TImage>
        where TImage : class
    {
        TImage From(Stream stream, ImageSize requestedSize);

        void To(Stream stream, TImage image);
    }
}
