using ImageMagick;
using MvvmScarletToolkit.Observables;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Samples
{
    public sealed class Image : ViewModelBase
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

        private BitmapSource _source;
        public BitmapSource Source
        {
            get { return _source; }
            private set { SetValue(ref _source, value); }
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

            var image = await Task.Run(() => GetImage(_path), token);
            await Dispatcher.Invoke(() => Source = image);
        }

        private static BitmapSource GetImage(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var img = new MagickImage(stream))
            {
                var source = img.ToBitmapSource();
                source.Freeze();

                return source;
            }
        }
    }
}
