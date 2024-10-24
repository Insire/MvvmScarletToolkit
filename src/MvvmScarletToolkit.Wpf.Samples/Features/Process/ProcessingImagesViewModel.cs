using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Process
{
    public partial class ProcessingImagesViewModel : ObservableObject
    {
        private ImagesViewModel _source;
        public ImagesViewModel Source
        {
            get => _source;
            private set => SetProperty(ref _source, value);
        }

        private ImagesViewModel _target;
        public ImagesViewModel Target
        {
            get => _target;
            private set => SetProperty(ref _target, value);
        }

        public ProcessingImagesViewModel(IScarletCommandBuilder commandBuilder, ImageViewModelProvider imageFactory)
        {
            _source = new ImagesViewModel(commandBuilder, imageFactory);
            _target = new ImagesViewModel(commandBuilder, imageFactory);
        }
    }
}
