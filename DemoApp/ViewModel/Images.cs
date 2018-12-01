using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public static async System.Threading.Tasks.Task<Images> FilledAsync()
        {
            var images = new Images(new ScarletDispatcher());
            await images.AddRange(ImageFactory.GetImageList()).ConfigureAwait(false);

            return images;
        }

        public Images(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }
    }
}
