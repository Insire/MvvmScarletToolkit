using MvvmScarletToolkit;

namespace DemoApp
{
    public partial class MainWindow : ConfigurableWindow
    {
        private IConfigurableWindowSettings _settings;
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override IConfigurableWindowSettings CreateSettings()
        {
            return _settings = _settings ?? new MainWindowSettings(this);
        }
    }
}
