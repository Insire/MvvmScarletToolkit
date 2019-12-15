namespace MvvmScarletToolkit.Observables
{
    public class ViewModelContainer<T> : ObservableObject
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetValue(ref _value, value); }
        }

        public ViewModelContainer(T value)
        {
            _value = value;
        }
    }
}
