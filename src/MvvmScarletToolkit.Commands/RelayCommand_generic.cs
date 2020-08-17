using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public sealed class RelayCommand<TArgument> : ICommand
    {
        private readonly Action<TArgument>? _execute;
        private readonly Func<TArgument, bool>? _canExecute;
        private readonly IScarletCommandManager _commandManager;

        public RelayCommand(in IScarletCommandBuilder? commandBuilder, in Action<TArgument> methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(in IScarletCommandBuilder? commandBuilder, in Action<TArgument> methodToExecute, in Func<TArgument, bool> canExecuteEvaluator)
            : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(in IScarletCommandManager? commandManager, in Action<TArgument> execute)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _execute = execute ?? throw new ArgumentNullException($"{nameof(execute)} can't be empty.", nameof(execute));
        }

        public RelayCommand(in IScarletCommandManager? commandManager, in Action<TArgument> execute, in Func<TArgument, bool> canExecute)
            : this(commandManager, execute)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute is null)
            {
                return true;
            }

            if (parameter is null)
            {
                if (typeof(TArgument).IsValueType)
                {
                    return false;
                }

                return _canExecute.Invoke(default!);
            }
            else
            {
                return _canExecute.Invoke((TArgument)parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (parameter is null)
            {
                _execute?.Invoke(default!);
            }
            else
            {
                _execute?.Invoke((TArgument)parameter);
            }
        }
    }
}
