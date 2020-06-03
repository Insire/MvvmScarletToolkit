using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Samples
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
