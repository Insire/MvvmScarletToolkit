using MvvmScarletToolkit;
using MvvmScarletToolkit.Commands;
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
            var commandBuilder = new CommandBuilder(dispatcher, commandManager);

            var scenes = new[]
            {
                new Scene()
                {
                    Content = new ParentViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new Images(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProcessingImagesViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DataContextSchenanigansViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new AsyncCommandViewModelStuff(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ParentsViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new ProgressViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new DummySnakeViewModel(),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new FileSystemViewModel(commandBuilder, new FileSystemOptionsViewModel(commandBuilder)),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content = new BusyViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene()
                {
                    Content= new TextDisplayViewModel(commandBuilder),
                    IsSelected = true,
                }
            };

            var navigation = new NavigationViewModel(commandBuilder);
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
