using MvvmScarletToolkit;

namespace DemoApp
{
    public class Image : ObservableObject, IOrderable
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetValue(ref _path, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        private int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set { SetValue(ref _sequence, value); }
        }
    }
}
