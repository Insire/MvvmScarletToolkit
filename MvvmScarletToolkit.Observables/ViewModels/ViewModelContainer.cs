namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Generic wrapper viewmodel to add binding support
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
