using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IConcurrentCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);
    }
}
