using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit.Observables
{
    public class EnumViewModel<T> : ViewModelContainer<T>
        where T : Enum
    {
        public EnumViewModel(in T value, in string? displayName, in IMessenger  messenger)
            : base(value, messenger)
        {
            DisplayName = displayName ?? "Undefined";
        }
    }
}
