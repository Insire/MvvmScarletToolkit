using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public abstract class ScarletFileSystemBase : ObservableObject
    {
        protected readonly BusyStack _busyStack;

        public ICommand LoadCommand { get; protected set; }
        public ICommand ClearCommand { get; protected set; }

        private bool _exists;
        public bool Exists
        {
            get { return _exists; }
            protected set { SetValue(ref _exists, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetValue(ref _isLoaded, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetValue(ref _isExpanded, value, Changed: OnLoadingChanged); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        private IDepth _depth;
        public IDepth Depth
        {
            get { return _depth; }
            protected set { SetValue(ref _depth, value); }
        }

        protected ScarletFileSystemBase()
        {
            _busyStack = new BusyStack();
            _busyStack.OnChanged += (hasItems) => IsBusy = hasItems;

            LoadCommand = new RelayCommand(Load, CanLoad);
        }

        protected virtual void OnLoadingChanged()
        {
            if (IsLoaded)
                Load();
        }

        public abstract void Load();

        protected bool CanLoad()
        {
            return !IsBusy && !IsLoaded;
        }
    }
}
