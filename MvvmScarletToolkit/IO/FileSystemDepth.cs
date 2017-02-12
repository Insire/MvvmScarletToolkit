namespace MvvmScarletToolkit
{
    public class FileSystemDepth : ObservableObject, IDepth
    {
        public bool CanLoad => Depth <= MaxDepth;

        private int _maxDepth;
        public int MaxDepth
        {
            get { return _maxDepth; }
            private set { SetValue(ref _maxDepth, value); }
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            set { SetValue(ref _depth, value); }
        }

        public FileSystemDepth(int maxDepth)
        {
            MaxDepth = maxDepth;
            Depth = 0;
        }
    }
}
