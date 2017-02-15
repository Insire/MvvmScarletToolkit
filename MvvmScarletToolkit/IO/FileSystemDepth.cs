namespace MvvmScarletToolkit
{
    public class FileSystemDepth : ObservableObject, IDepth
    {
        public bool IsMaxReached => Current > Maximum;

        private int _maxDepth;
        public int Maximum
        {
            get { return _maxDepth; }
            private set { SetValue(ref _maxDepth, value, Changed: OnIsMaxReachedChanged); }
        }

        private int _depth;
        public int Current
        {
            get { return _depth; }
            set { SetValue(ref _depth, value, Changed: OnIsMaxReachedChanged); }
        }

        public FileSystemDepth(int maxDepth)
        {
            Maximum = maxDepth;
            Current = 0;
        }

        private void OnIsMaxReachedChanged()
        {
            OnPropertyChanged(nameof(IsMaxReached));
        }
    }
}
