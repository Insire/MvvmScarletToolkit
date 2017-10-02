using MvvmScarletToolkit;
using System.Diagnostics;
using System.Windows.Input;

namespace DemoApp
{
    public class DemoItem : ObservableObject
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

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set { SetValue(ref _isLoaded, value); }
        }

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get { return _loadCommand; }
            private set { SetValue(ref _loadCommand, value); }
        }

        public DemoItem()
        {
            DisplayName = "unknown";
            LoadCommand = new RelayCommand(Load, () => !IsLoaded);
        }

        public DemoItem(string displayName) : this()
        {
            DisplayName = displayName;
        }

        private void Load()
        {
            Debug.WriteLine("loading");

            IsLoaded = true;
        }
    }
}
