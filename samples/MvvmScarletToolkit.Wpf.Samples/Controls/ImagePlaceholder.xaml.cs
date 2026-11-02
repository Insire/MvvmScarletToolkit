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

        /// <summary>
        /// The size in pixels, that the image is decoded to fit into. Should match the displayed size, larger values waste memory.
        /// </summary>
        public uint DecodeSize
        {
            get { return (uint)GetValue(DecodeSizeProperty); }
            set { SetValue(DecodeSizeProperty, value); }
        }

        public static readonly DependencyProperty DecodeSizeProperty = DependencyProperty.Register(
            nameof(DecodeSize),
            typeof(uint),
            typeof(ImagePlaceholder),
            new PropertyMetadata(800u));
    }
}
