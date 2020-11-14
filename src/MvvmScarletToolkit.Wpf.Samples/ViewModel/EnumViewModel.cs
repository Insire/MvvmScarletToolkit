using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class EnumViewModel : ObservableObject
    {
        private ViewModelEnum _value;
        public ViewModelEnum Value
        {
            get { return _value; }
            set { SetValue(ref _value, value); }
        }
    }
}
