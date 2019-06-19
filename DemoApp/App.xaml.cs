using MvvmScarletToolkit;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Windows;

namespace DemoApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var commandManager = new ScarletCommandManager();
            var dispatcher = ScarletDispatcher.Default;
            var commandBuilder = new CommandBuilder(dispatcher, commandManager, (lambda) => new BusyStack(lambda, dispatcher));
            var navigation = new NavigationViewModel(commandBuilder, new LocalizationsViewModel(new LocalizationProvider()));

            var window = new MainWindow
            {
                DataContext = navigation
            };

            window.Show();
        }
    }
}
