using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Collections.Generic;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public static Images Empty => new Images(new ScarletDispatcher());

        public static Images Filled => new Images(ImageFactory.GetImageList(), new ScarletDispatcher());

        public Images(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        public Images(IEnumerable<Image> items, IScarletDispatcher dispatcher)
            : base(items, dispatcher)
        {
        }
    }
}
