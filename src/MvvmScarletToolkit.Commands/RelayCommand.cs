using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;
        private readonly IScarletCommandManager _commandManager;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public RelayCommand(in IScarletCommandBuilder? commandBuilder, in Action methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(in IScarletCommandBuilder? commandBuilder, in Action methodToExecute, in Func<bool> canExecuteEvaluator)
            : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(in IScarletCommandManager? commandManager, in Action methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentNullException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
            _commandManager = commandManager ?? throw new ArgumentNullException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
        }

        public RelayCommand(in IScarletCommandManager? commandManager, in Action methodToExecute, in Func<bool>? canExecute)
            : this(commandManager, methodToExecute)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute is null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
        }
    }
}
