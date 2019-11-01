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

            var commandBuilder = new CommandBuilder(ScarletDispatcher.Default, ScarletCommandManager.Default, ScarletMessenger.Default, ExitService.Default, ScarletWeakEventManager.Default, (lambda) => new BusyStack(lambda, ScarletDispatcher.Default));
            var navigation = new NavigationViewModel(commandBuilder, new LocalizationsViewModel(new LocalizationProvider()));

            var window = new MainWindow
            {
                DataContext = navigation
            };

            window.Show();
        }
    }
}
