using GraphicsMagick;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit
{
    public partial class MagickImageControl : INotifyPropertyChanged
    {
        private static ConcurrentDictionary<string, WeakReference<BitmapSource>> _inMemoryCache;
        private readonly object _syncRoot = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private BusyStack _busyStack;
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set
            {
                if (_busyStack == value)
                    return;

                _busyStack = value;
                OnPropertyChanged(nameof(BusyStack));
            }
        }

        private BitmapSource _image;
        public BitmapSource Image
        {
            get { return _image; }
            private set
            {
                if (_image == value)
                    return;

                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        static MagickImageControl()
        {
            _inMemoryCache = new ConcurrentDictionary<string, WeakReference<BitmapSource>>();
        }

        public MagickImageControl()
        {
            InitializeComponent();

            BusyStack = new BusyStack();
            BusyStack.OnChanged = (hasItems) => IsBusy = hasItems;
        }

        private async void MagickImageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsBusy)
                return;

            using (BusyStack.GetToken())
            {
                try
                {
                    Image = await AnalyzeContext(e);
                }
                catch (UnauthorizedAccessException)
                {
                    Image = default(BitmapSource);

                    // this exception should be logged, instead of being ignored
                }
            }
        }

        private Task<BitmapSource> AnalyzeContext(DependencyPropertyChangedEventArgs e)
        {
            var path = e.NewValue as string;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return LoadImage(path);

            // info is not threadsafe, so only use it as datastore, dont use its methods
            if (e.NewValue is FileInfo info && File.Exists(info.FullName))
                return LoadImage(info.FullName);

            return Task.FromResult(default(BitmapSource));
        }

        private Task<BitmapSource> LoadImage(string path)
        {
            return Task.Run(() =>
            {
                if (_inMemoryCache.ContainsKey(path))
                {
                    lock (_syncRoot)
                    {
                        return LoadImageInternalFromCache(path);
                    }
                }
                else
                    return LoadImageInternalFromCache(path);
            });
        }

        private BitmapSource LoadImageInternalFromCache(string path)
        {
            var source = default(BitmapSource);
            var reference = default(WeakReference<BitmapSource>);
            var success = false;
            do
            {
                reference = GetFromCache(path);
                success = reference.TryGetTarget(out source);
                if (!success)
                    _inMemoryCache.TryRemove(path, out reference);
            }
            while (!success);

            return source;
        }

        private WeakReference<BitmapSource> GetFromCache(string path)
        {
            var factory = new Func<WeakReference<BitmapSource>>(() =>
            {
                var sourceInt = LoadImageInternal(path);
                return new WeakReference<BitmapSource>(sourceInt);
            });

            return _inMemoryCache.GetOrAdd(path, factory());
        }

        private BitmapSource LoadImageInternal(string path)
        {
            Debug.WriteLine(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var settings = MagickReadSettingsFactory.GetSettings(stream);
                var bounds = new MagickGeometry(Screen.PrimaryScreen.Bounds);
                bounds.IgnoreAspectRatio = false;

                using (var image = new MagickImage(stream, settings))
                {
                    if (image.Width > bounds.Width || image.Height > bounds.Height)
                        image.Resize(bounds);

                    var bitmapSource = image.ToBitmapSource();
                    bitmapSource.Freeze();

                    return bitmapSource;
                }
            }
        }
    }
}
