using System.Diagnostics;

namespace DemoApp
{
    public partial class DataContextSchenanigans
    {
        public DataContextSchenanigans()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("UserControl_DataContextChanged");
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Debug.WriteLine("UserControl_Loaded");
        }
    }
}
