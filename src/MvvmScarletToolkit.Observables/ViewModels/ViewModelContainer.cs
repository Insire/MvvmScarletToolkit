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
        public partial T Value { get; set; }

        public ViewModelContainer(in T value)
        {
            Value = value;
        }
    }
}
