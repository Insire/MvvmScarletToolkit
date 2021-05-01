using Jot;
using Jot.Storage;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public partial class App : Application
    {
        private readonly Tracker _tracker;

        public App()
        {
            _tracker = new Tracker(new JsonFileStore(Environment.SpecialFolder.ApplicationData));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _tracker.Configure<MainWindow>()
                .Id(w => w.Name, $"[Width={SystemParameters.VirtualScreenWidth},Height{SystemParameters.VirtualScreenHeight}]")
                .Properties(w => new { w.Height, w.Width, w.Left, w.Top, w.WindowState })
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            var navigation = new NavigationViewModel(SynchronizationContext.Current, ScarletCommandBuilder.Default, new LocalizationsViewModel(new ScarletLocalizationProvider()));

            var window = new MainWindow(_tracker, navigation);

            window.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            await ScarletExitService.Default.ShutDown();

            _tracker.PersistAll();
        }
    }
}
