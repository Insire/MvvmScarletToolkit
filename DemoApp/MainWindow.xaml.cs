using Jot;

namespace DemoApp
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
