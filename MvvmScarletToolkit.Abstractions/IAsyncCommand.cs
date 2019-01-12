using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IConcurrentCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
