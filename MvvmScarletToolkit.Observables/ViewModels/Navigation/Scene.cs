using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    public class Scene : ObservableObject
    {
        private INotifyPropertyChanged _content;
        public INotifyPropertyChanged Content
        {
            get { return _content; }
            set { SetValue(ref _content, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }
    }
}
