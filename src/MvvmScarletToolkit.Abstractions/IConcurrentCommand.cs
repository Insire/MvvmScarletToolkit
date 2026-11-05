using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IConcurrentCommand : ICommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Explains why the most recent <see cref="ICommand.CanExecute(object)"/> call returned <see langword="false"/>.
        /// <see langword="null"/> while the command can execute or when no reason is available.
        /// </summary>
        string? DisabledReason { get; }

        Task ExecuteAsync(object parameter);
    }
}
