namespace MvvmScarletToolkit.SnakeGame
{
    public class Size : ObservableObject
    {
        private int _width;
        public int Width
        {
            get { return _width; }
            protected set { SetValue(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            protected set { SetValue(ref _height, value); }
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
