using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public partial class ProcessingImagesViewModel : ObservableObject
    {
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
            Source = new ImagesViewModel(commandBuilder, imageFactory);
            Target = new ImagesViewModel(commandBuilder, imageFactory);
        }
    }
}
