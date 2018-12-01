using MvvmScarletToolkit.Observables;
using System.Collections.Generic;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public static Images Empty => new Images();

        public static Images Filled => new Images(ImageFactory.GetImageList());

        public Images()
        {
        }

        public Images(IEnumerable<Image> items)
            : base(items)
        {
        }
    }
}
