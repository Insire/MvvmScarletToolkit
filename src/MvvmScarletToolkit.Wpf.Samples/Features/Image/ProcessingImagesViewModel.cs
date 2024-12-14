using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class ProcessingImagesViewModel : ObservableObject
    {
        private readonly ImageViewModelProvider _imageFactory;

        private SourceImagesViewModel _source;
        public SourceImagesViewModel Source
        {
            get { return _source; }
            private set { SetProperty(ref _source, value); }
        }

        private TargetImagesViewModel _target;
        public TargetImagesViewModel Target
        {
            get { return _target; }
            private set { SetProperty(ref _target, value); }
        }

        public ProcessingImagesViewModel(IScarletCommandBuilder commandBuilder, ImageViewModelProvider imageFactory)
        {
            _target = new TargetImagesViewModel(commandBuilder);
            _source = new SourceImagesViewModel(commandBuilder, imageFactory, _target);
            _imageFactory = imageFactory;
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            return _imageFactory.Initialize(cancellationToken);
        }
    }
}
