using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [DebuggerDisplay("Size {Width};{Height}")]
    public sealed class Size : ObservableObject
    {
        private int _width;
        public int Width
        {
            get { return _width; }
            private set { SetProperty(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            private set { SetProperty(ref _height, value); }
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
