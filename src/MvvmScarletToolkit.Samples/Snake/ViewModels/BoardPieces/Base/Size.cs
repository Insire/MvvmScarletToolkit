using MvvmScarletToolkit.Observables;
using System.Diagnostics;

namespace MvvmScarletToolkit.Samples
{
    [DebuggerDisplay("Size {Width};{Height}")]
    public sealed class Size : ObservableObject
    {
        private int _width;
        public int Width
        {
            get { return _width; }
            private set { SetValue(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            private set { SetValue(ref _height, value); }
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
