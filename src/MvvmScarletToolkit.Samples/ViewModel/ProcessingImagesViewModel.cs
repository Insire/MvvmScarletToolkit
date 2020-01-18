using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Samples
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

        public ProcessingImagesViewModel(ICommandBuilder commandBuilder)
        {
            Source = new Images(commandBuilder);
            Target = new Images(commandBuilder);
        }
    }
}
