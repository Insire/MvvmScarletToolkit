using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IExtendedAsyncCommand : IAsyncCommand, INotifyPropertyChanged
    {
        ICommand CancelCommand { get; }
        Task Completion { get; }

        bool IsBusy { get; }
    }
}
