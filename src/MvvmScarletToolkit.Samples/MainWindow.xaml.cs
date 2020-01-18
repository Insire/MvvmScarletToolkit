using Jot;

namespace MvvmScarletToolkit.Samples
{
    public partial class MainWindow
    {
        public MainWindow(Tracker tracker)
        {
            InitializeComponent();

            tracker.Track(this);
        }
    }
}
