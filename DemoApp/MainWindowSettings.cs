using DemoApp.Properties;
using MvvmScarletToolkit.ConfigurableWindow;

namespace DemoApp
{
    public sealed class MainWindowSettings : ConfigurableWindowSettings
    {
        // the values of these consts have to be added in the project settings with their according type set
        // so, IsFirstRun has to be added and be set as a bool for instance

        private const string IS_FIRST_RUN = "IsFirstRun";
        private const string WINDOW_LOCATION = "MainWindowLocation";
        private const string WINDOW_SIZE = "MainWindowSize";
        private const string WINDOW_STATE = "MainWindowState";

        public MainWindowSettings(MainWindow window)
                : base(
                Settings.Default,
                IS_FIRST_RUN,
                WINDOW_LOCATION,
                WINDOW_SIZE,
                WINDOW_STATE)
        {
            // Note: You only want to have this code
            // in the application's main Window, not
            // in dialog boxes or other child Windows.
            window.Closed += delegate
            {
                if (IsFirstRun)
                {
                    IsFirstRun = false;
                }
            };
        }
    }
}
