using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed class ProcessingImagesViewModel : ObservableObject
    {
        private readonly ImageViewModelProvider _imageFactory;

        private ImagesViewModel _source;
        public ImagesViewModel Source
        {
            get { return _source; }
            private set { SetProperty(ref _source, value); }
        }

        private ImagesViewModel _target;

        public ImagesViewModel Target
        {
            get { return _target; }
            private set { SetProperty(ref _target, value); }
        }

        public ProcessingImagesViewModel(IScarletCommandBuilder commandBuilder, ImageViewModelProvider imageFactory)
        {
            _source = new ImagesViewModel(commandBuilder, imageFactory);
            _target = new ImagesViewModel(commandBuilder, imageFactory);
            _imageFactory = imageFactory;
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            return _imageFactory.Initialize(cancellationToken);
        }
    }
}
