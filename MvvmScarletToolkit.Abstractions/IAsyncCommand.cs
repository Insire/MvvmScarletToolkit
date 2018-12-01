using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
