using MvvmScarletToolkit.Observables;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class Image : ViewModelBase
    {
        private readonly Assembly _assembly;

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

        public Image(IScarletCommandBuilder commandBuilder, Assembly assembly)
            : base(commandBuilder)
        {
            LoadCommand = commandBuilder.Create(Load, () => true)
                .WithSingleExecution()
                .Build();

            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        private async Task Load(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_path) || _source != null)
            {
                return;
            }

            var image = await Task.Run(() => GetImage(_path), token).ConfigureAwait(false);
            await Dispatcher.Invoke(() => Source = image).ConfigureAwait(false);
        }

        private  BitmapSource GetImage(string resourceName)
        {
            using (var stream = _assembly.GetManifestResourceStream(resourceName))
            {
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
}
