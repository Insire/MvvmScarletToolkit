using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IAsyncCommand : ICommand
    {
       Task ExecuteAsync(object parameter);
    }
}
