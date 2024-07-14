using System;
using System.IO;

namespace MvvmScarletToolkit.Abstractions.ImageLoading
{
    public interface IImageFactory<T>
    {
        T From(Stream stream, ImageSize requestedSize);

        T From(Uri uri, ImageSize requestedSize);

        T From(string uri, ImageSize requestedSize);
    }
}
