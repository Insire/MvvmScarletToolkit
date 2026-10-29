using Jot;
using Microsoft.Win32;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Wpf.Samples.Features;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public partial class MainWindow
    {
        public Tracker Tracker
        {
            get { return (Tracker)GetValue(TrackerProperty); }
            set { SetValue(TrackerProperty, value); }
        }

        public static readonly DependencyProperty TrackerProperty = DependencyProperty.Register(
            nameof(Tracker),
            typeof(Tracker),
            typeof(MainWindow),
            new PropertyMetadata(null));

        public MainWindow(Tracker tracker, NavigationViewModel navigationViewModel)
        {
            ArgumentNullException.ThrowIfNull(tracker, nameof(tracker));
            ArgumentNullException.ThrowIfNull(navigationViewModel, nameof(navigationViewModel));

            InitializeComponent();

            tracker.Track(this);

            SetCurrentValue(TrackerProperty, tracker);

            DataContext = navigationViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void SelectProcessFile_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ProcessViewModel viewModel)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe;*.cmd;*.bat)|*.exe;*.cmd;*.bat|All files (*.*)|*.*",
            };

            if (dialog.ShowDialog(this) == true)
            {
                viewModel.FilePath = dialog.FileName;
            }
        }
    }
}
