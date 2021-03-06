using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class EnumViewModel : ObservableObject
    {
        private ViewModelEnum _value;
        public ViewModelEnum Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}
