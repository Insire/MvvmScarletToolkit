using System.Windows;

namespace DemoApp
{
    public partial class AnotherImageControl
    {
        public AnotherImageControl()
        {
            InitializeComponent();
        }

        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(AnotherImageControl), new PropertyMetadata(string.Empty));
    }
}
