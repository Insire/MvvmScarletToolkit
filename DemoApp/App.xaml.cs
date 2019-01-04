using MvvmScarletToolkit;
using MvvmScarletToolkit.FileSystemBrowser;
using MvvmScarletToolkit.Implementations;
using MvvmScarletToolkit.Observables;
using System.Windows;

namespace DemoApp
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var commandManager = new ScarletCommandManager();
            var dispatcher = new ScarletDispatcher();
            var scenes = new[]
            {
                new Scene()
                {
                    Content = new ParentViewModel(dispatcher,commandManager),
                    IsSelected = true,
                },
                new Scene()
                {
                    Content = new Images(dispatcher,commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProcessingImagesViewModel(dispatcher,commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DataContextSchenanigansViewModel(dispatcher,commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new AsyncCommandViewModelStuff(commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ParentsViewModel(dispatcher,commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProgressViewModel(commandManager),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DummySnakeViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new FileSystemViewModel(dispatcher,commandManager, new FileSystemOptionsViewModel(dispatcher,commandManager)),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new BusyViewModel(dispatcher,commandManager),
                    IsSelected = false,
                }
            };

            var navigation = new NavigationViewModel(dispatcher, commandManager);
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
