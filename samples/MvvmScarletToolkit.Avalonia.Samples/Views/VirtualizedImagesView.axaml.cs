using Avalonia.Controls;
using MvvmScarletToolkit.Avalonia.Samples.Behaviors;
using MvvmScarletToolkit.Core.Samples.Features.Image;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class VirtualizedImagesView : UserControl
    {
        private readonly ContainerLifecycle<VirtualizedImageViewModel> _lifecycle;

        public VirtualizedImagesView()
        {
            InitializeComponent();

            _lifecycle = new ContainerLifecycle<VirtualizedImageViewModel>(Images, item => item.RealizeCommand.Execute(null), item => item.UnrealizeCommand.Execute(null));
        }
    }
}
