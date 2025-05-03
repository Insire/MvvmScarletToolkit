using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Samples.Controls
{
    public partial class ImagePlaceholder : UserControl
    {
        public ImagePlaceholder()
        {
            InitializeComponent();
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(ImagePlaceholder),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
