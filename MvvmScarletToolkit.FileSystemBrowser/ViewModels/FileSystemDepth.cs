using MvvmScarletToolkit.Observables;
using System.ComponentModel;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemDepth : ObservableObject, IDepth
    {
        public bool IsMaxReached => Current > Maximum;

        private int _maximum;
        [Bindable(true, BindingDirection.OneWay)]
        public int Maximum
        {
            get { return _maximum; }
            private set { SetValue(ref _maximum, value, OnChanged: OnIsMaxReachedChanged); }
        }

        private int _current;
        [Bindable(true, BindingDirection.OneWay)]
        public int Current
        {
            get { return _current; }
            set { SetValue(ref _current, value, OnChanged: OnIsMaxReachedChanged); }
        }

        public FileSystemDepth(int maximum)
        {
            Maximum = maximum;
            Current = 0;
        }

        private void OnIsMaxReachedChanged()
        {
            OnPropertyChanged(nameof(IsMaxReached));
        }
    }
}
