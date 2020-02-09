using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions.Extensions
{
    public static class CommandExtensions
    {
        public static void TryExecute(this ICommand command, object parameter)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
