using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed partial class Image : ViewModelBase
    {
        private readonly Assembly _assembly;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _path;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _sequence;

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
            if (string.IsNullOrEmpty(Path) || _source != null)
            {
                return;
            }

            var image = await Task.Run(() => GetImage(Path), token).ConfigureAwait(false);
            await Dispatcher.Invoke(() => Source = image).ConfigureAwait(false);
        }

        private BitmapSource GetImage(string resourceName)
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
