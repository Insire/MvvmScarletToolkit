using GraphicsMagick;
using System;
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
            BusyStack.OnChanged = (hasItems) =>
            {
                IsBusy = hasItems;
            };
        }

        private async void MagickImageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsBusy)
                return;

            using (BusyStack.GetToken())
            {
                CollectOldData();

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

            var info = e.NewValue as FileInfo; // info is not threadsafe, so only use it as datastore, dont use its methods
            if (info != null && File.Exists(info.FullName))
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
            return Task.Run(() =>
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var settings = MagickReadSettingsFactory.GetSettings(stream);
                    var bounds = new MagickGeometry(Screen.PrimaryScreen.Bounds);
                    bounds.IgnoreAspectRatio = false;

                    using (var image = new MagickImage(stream, settings))
                    {
                        if (image.Width < bounds.Width || image.Height < bounds.Height)
                            image.Resize(bounds);

                        var bitmapSource = image.ToBitmapSource();
                        bitmapSource.Freeze();

                        return bitmapSource;
                    }
                }
            });
        }

        private void CollectOldData()
        {
            // the bitmap source can stay quite a while in memory, for some reason...
            // this forces it out of memory sooner
            var oldValue = Image;

            if (oldValue == null) // only call the garbage collector if there was actually an image loaded
                return;

            Image = default(BitmapSource); // fancy way of setting the image to null
            GC.Collect();
        }
    }
}
