using Jot;
using Jot.Storage;
using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Windows;

namespace MvvmScarletToolkit.Samples
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
                .Id(w => w.Name, System.Windows.Forms.SystemInformation.VirtualScreen.Size)
                .Properties(w => new { w.Height, w.Width, w.Left, w.Top, w.WindowState })
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            var navigation = new NavigationViewModel(ScarletCommandBuilder.Default, new LocalizationsViewModel(new ScarletLocalizationProvider()));

            var window = new MainWindow(_tracker)
            {
                DataContext = navigation
            };

            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _tracker.PersistAll();
        }
    }
}
