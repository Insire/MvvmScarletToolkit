using MvvmScarletToolkit.Observables;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class Image : ViewModelBase
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private int _sequence;

        public int Sequence
        {
            get { return _sequence; }
            set { SetProperty(ref _sequence, value); }
        }

        private BitmapSource _source;
        public BitmapSource Source
        {
            get { return _source; }
            private set { SetProperty(ref _source, value); }
        }

        public ICommand LoadCommand { get; }

        public Image(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            LoadCommand = commandBuilder.Create(Load, () => true)
                .WithSingleExecution()
                .Build();
        }

        private async Task Load(CancellationToken token)
        {
            if (!File.Exists(_path) || _source != null)
            {
                return;
            }

            var image = await Task.Run(() => GetImage(_path), token).ConfigureAwait(false);
            await Dispatcher.Invoke(() => Source = image).ConfigureAwait(false);
        }

        private static BitmapSource GetImage(string path)
        {
            using var stream = File.OpenRead(path);

            var img = new BitmapImage();
            img.BeginInit();
            img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();

            img.Freeze();

            return img;
        }
    }
}
