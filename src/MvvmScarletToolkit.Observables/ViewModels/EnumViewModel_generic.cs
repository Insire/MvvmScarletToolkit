using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    public partial class EnumViewModel<T> : ViewModelContainer<T>
        where T : Enum
    {
        [ObservableProperty]
        public partial string DisplayName { get; set; }

        public EnumViewModel(in T value, in string? displayName)
            : base(value)
        {
            DisplayName = displayName ?? "Undefined";
        }
    }
}
