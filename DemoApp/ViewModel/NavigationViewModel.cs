using MvvmScarletToolkit;
using MvvmScarletToolkit.FileSystemBrowser;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel()
            : base(new[]
            {
                new Scene()
                {
                    Content = new ParentViewModel(),
                    IsSelected = true,
                },
                new Scene()
                {
                    Content = Images.Filled,
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProcessingImagesViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DataContextSchenanigansViewModel(new ScarletDispatcher()),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new AsyncCommandViewModelStuff(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ParentsViewModel(new ScarletDispatcher()),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProgressViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DummySnakeViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new FileSystemViewModel(),
                    IsSelected = false,
                },
            }, new ScarletDispatcher())
        {
        }
    }
}
