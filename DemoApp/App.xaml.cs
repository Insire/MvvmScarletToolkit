using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
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
            var commandManager = new ScarletCommandManager();
            var dispatcher = ScarletDispatcher.Default;
            var commandBuilder = new CommandBuilder(dispatcher, commandManager, (lambda) => new BusyStack(lambda, dispatcher));
            var localizationService = default(ILocalizationService);

            var scenes = new[]
            {
                new Scene(commandBuilder,localizationService.CreateViewModel("ParentViewModel"))
                {
                    Content = new ParentViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Image loading"))
                {
                    Content = new Images(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Drag and Drop"))
                {
                    Content = new ProcessingImagesViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("DataContextSchenanigansViewModel"))
                {
                    Content = new DataContextSchenanigansViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("AsyncCommands"))
                {
                    Content = new AsyncCommandViewModelStuff(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Datagrid"))
                {
                    Content = new ParentsViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Progress"))
                {
                    Content = new ProgressViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Snake"))
                {
                    Content = new DummySnakeViewModel(),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("FileSystemBrowser"))
                {
                    Content = new FileSystemViewModel(commandBuilder, new FileSystemOptionsViewModel(commandBuilder)),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Busy tree"))
                {
                    Content = new BusyViewModel(commandBuilder),
                    IsSelected = false,
                },
                new Scene(commandBuilder,localizationService.CreateViewModel("Text rendering"))
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
