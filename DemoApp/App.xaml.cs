using MvvmScarletToolkit;
using MvvmScarletToolkit.FileSystemBrowser;
using MvvmScarletToolkit.Observables;
using System.Windows;

namespace DemoApp
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dispatcher = new ScarletDispatcher();
            var scenes = new[]
            {
                new Scene()
                {
                    Content = new ParentViewModel(),
                    IsSelected = true,
                },
                new Scene()
                {
                    Content = new Images(dispatcher),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProcessingImagesViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DataContextSchenanigansViewModel(dispatcher),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new AsyncCommandViewModelStuff(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ParentsViewModel(dispatcher),
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
                    Content = new FileSystemViewModel(dispatcher),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new BusyViewModel(dispatcher),
                    IsSelected = false,
                }
            };

            var navigation = new NavigationViewModel();
            await navigation.AddRange(scenes).ConfigureAwait(false);

            await dispatcher.Invoke(() =>
             {
                 var window = new MainWindow
                 {
                     DataContext = navigation
                 };

                 window.Show();
             }).ConfigureAwait(false);
        }
    }
}
