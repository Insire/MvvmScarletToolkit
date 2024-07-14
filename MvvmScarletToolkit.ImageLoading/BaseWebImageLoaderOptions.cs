namespace MvvmScarletToolkit.ImageLoading
{
    public class BaseWebImageLoaderOptions
    {
        public bool DisposeHttpClient { get; init; } = false;

        public int DefaultHeight { get; init; } = 300;
        public int DefaultWidth { get; init; } = 300;
    }
}
