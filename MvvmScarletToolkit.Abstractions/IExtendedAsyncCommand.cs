using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IExtendedAsyncCommand : IAsyncCommand, IBusy
    {
        ICommand CancelCommand { get; }
        Task Completion { get; }
    }
}
