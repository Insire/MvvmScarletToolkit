using Jot;
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
            if (navigationViewModel is null)
            {
                throw new System.ArgumentNullException(nameof(navigationViewModel));
            }

            InitializeComponent();

            tracker.Track(this);

            SetCurrentValue(TrackerProperty, tracker);

            DataContext = navigationViewModel;
        }
    }
}
