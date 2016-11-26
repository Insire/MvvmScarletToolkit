using GraphicsMagick;
using System.Collections.Generic;
using System.ComponentModel;
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
                if (EqualityComparer<bool>.Default.Equals(_isBusy, value))
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
                if (EqualityComparer<BusyStack>.Default.Equals(_busyStack, value))
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
                if (EqualityComparer<BitmapSource>.Default.Equals(_image, value))
                    return;

                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public MagickImageControl()
        {
            InitializeComponent();

            BusyStack = new BusyStack();
            BusyStack.CollectionChanged += BusyStack_Changed;
        }

        private void BusyStack_Changed(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsBusy = BusyStack.HasItems();
        }

        private async void MagickImageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            using (BusyStack.GetToken())
                Image = await AnalyzeContext(e);
        }

        private Task<BitmapSource> AnalyzeContext(DependencyPropertyChangedEventArgs e)
        {
            var path = e.NewValue as string;
            if (!string.IsNullOrEmpty(path))
                return LoadImage(path);

            var info = e.NewValue as FileInfo;
            if (info != null)
                return LoadImage(info);

            return Task.FromResult(default(BitmapSource));
        }

        private Task<BitmapSource> LoadImage(string path)
        {
            return LoadImageInternal(path);
        }

        private Task<BitmapSource> LoadImage(FileInfo info)
        {
            return LoadImageInternal(info.FullName);
        }

        private Task<BitmapSource> LoadImageInternal(string path)
        {
            var settings = new MagickReadSettings
            {
                Format = MagickFormat.Jpeg,
            };

            var bounds = new MagickGeometry(Screen.PrimaryScreen.Bounds);
            bounds.IgnoreAspectRatio = false;

            return Task.Run(() =>
            {
                using (var image = new MagickImage(path, settings))
                {
                    if (image.Width < bounds.Width || image.Height < bounds.Height)
                        image.Resize(bounds);

                    var bitmapSource = image.ToBitmapSource();
                    bitmapSource.Freeze();
                    return bitmapSource;
                }
            });
        }
    }
}

