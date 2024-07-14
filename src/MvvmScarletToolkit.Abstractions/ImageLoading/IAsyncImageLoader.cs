using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions.ImageLoading
{
    public interface IAsyncImageLoader<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Loads image
        /// </summary>
        /// <param name="url">Target url</param>
        /// <returns>Bitmap</returns>
        public Task<T?> ProvideImageAsync(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly);
    }
}
