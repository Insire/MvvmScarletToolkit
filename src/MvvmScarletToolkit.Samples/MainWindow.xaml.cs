using Jot;
using System.Windows;

namespace MvvmScarletToolkit.Samples
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

        public MainWindow(Tracker tracker)
        {
            InitializeComponent();

            tracker.Track(this);

            SetCurrentValue(TrackerProperty, tracker);
        }
    }
}
