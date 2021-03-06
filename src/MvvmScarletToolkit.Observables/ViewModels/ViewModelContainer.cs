using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Generic wrapper viewmodel to add binding support to any c# object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewModelContainer<T> : ObservableObject
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public ViewModelContainer(in T value)
        {
            _value = value;
        }
    }
}
