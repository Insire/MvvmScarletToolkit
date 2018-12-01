using MvvmScarletToolkit.Observables;
using System.Collections.Generic;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public static Images Empty
        {
            get
            {
                return new Images();
            }
        }

        public static Images Filled
        {
            get
            {
                return new Images(ImageFactory.GetImageList());
            }
        }

        public Images()
        {
        }

        public Images(IEnumerable<Image> items)
            : base(items)
        {
        }
    }
}
