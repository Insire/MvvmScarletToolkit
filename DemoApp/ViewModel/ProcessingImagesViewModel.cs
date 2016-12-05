using MvvmScarletToolkit;

namespace DemoApp
{
    public class ProcessingImagesViewModel : ObservableObject
    {
        private Images _source;
        public Images Source
        {
            get { return _source; }
            private set { SetValue(ref _source, value); }
        }

        private Images _target;
        public Images Target
        {
            get { return _target; }
            private set { SetValue(ref _target, value); }
        }

        public ProcessingImagesViewModel()
        {
            Source = ImageFactory.GetImages();
            Target = new Images();
        }
    }
}
