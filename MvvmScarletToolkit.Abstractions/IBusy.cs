using System.ComponentModel;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusy : INotifyPropertyChanged
    {
        bool IsBusy { get; }
    }
}
