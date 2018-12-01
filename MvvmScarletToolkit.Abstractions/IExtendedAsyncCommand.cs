using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IExtendedAsyncCommand : IAsyncCommand, IBusy
    {
        ICommand CancelCommand { get; }
    }
}
