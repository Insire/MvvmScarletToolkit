using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IConcurrentCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);
    }
}
