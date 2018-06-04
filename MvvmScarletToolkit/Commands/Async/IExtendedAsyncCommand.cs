using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IExtendedAsyncCommand : IAsyncCommand, IBusy
    {
        ICommand CancelCommand { get; }
    }
}
