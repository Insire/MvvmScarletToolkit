using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public partial class ProcessingImagesViewModel : ObservableObject
    {
        private Images _source;
        public Images Source
        {
            get { return _source; }
            private set { SetProperty(ref _source, value); }
        }

        private Images _target;
        public Images Target
        {
            get { return _target; }
            private set { SetProperty(ref _target, value); }
        }

        public ProcessingImagesViewModel(IScarletCommandBuilder commandBuilder, ImageFactory imageFactory)
        {
            Source = new Images(commandBuilder, imageFactory);
            Target = new Images(commandBuilder, imageFactory);
        }
    }
}
