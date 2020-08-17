using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public static class CommandExtensions
    {
        public static void TryExecute(this ICommand command, in object parameter)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
