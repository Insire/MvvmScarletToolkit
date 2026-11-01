using Avalonia;
using Avalonia.Controls;
using MvvmScarletToolkit.Core.Samples.Features.Image;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class ImagePlaceholder : UserControl
    {
        /// <summary>
        /// The size in pixels, that the image is decoded to fit into. Should match the displayed size, larger values waste memory.
        /// </summary>
        public static readonly StyledProperty<uint?> DecodeSizeProperty = AvaloniaProperty.Register<ImagePlaceholder, uint?>(nameof(DecodeSize), 800u);

        public uint? DecodeSize
        {
            get { return GetValue(DecodeSizeProperty); }
            set { SetValue(DecodeSizeProperty, value); }
        }

        public ImagePlaceholder()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            // paths are either urls or local files, a Uri handles both
            AsyncImageLoadingBehavior.SetSource(Image, DataContext is ImageViewModel { Path: { Length: > 0 } path } ? new Uri(path) : null);
        }
    }
}
