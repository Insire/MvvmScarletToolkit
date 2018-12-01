using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class DemoItem : ViewModelBase
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public DemoItem()
        {
            DisplayName = "unknown";
        }

        public DemoItem(string displayName) : this()
        {
            DisplayName = displayName;
        }
    }
}
