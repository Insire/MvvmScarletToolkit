using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Generic wrapper viewmodel to add binding support to any c# object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ViewModelContainer<T> : ObservableObject
    {
        [ObservableProperty]
        private T _value;

        public ViewModelContainer(in T value)
        {
            _value = value;
        }
    }
}
