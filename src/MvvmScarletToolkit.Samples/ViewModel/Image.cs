using MvvmScarletToolkit.Observables;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
{
    public sealed class Image : ObservableObject
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetValue(ref _path, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        private int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set { SetValue(ref _sequence, value); }
        }

        public IAsyncCommand BusyCommand { get; }

        public Image()
        {
            BusyCommand = AsyncCommand.Create(() => BeBusy());
        }

        private async Task BeBusy()
        {
            await Task.Delay(5000);
        }
    }
}
