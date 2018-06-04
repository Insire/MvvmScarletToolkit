using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public interface IBusy : INotifyPropertyChanged
    {
        bool IsBusy { get; }
    }
}
