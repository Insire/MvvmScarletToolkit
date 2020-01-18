using MvvmScarletToolkit.Abstractions;
using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;
        private readonly IScarletCommandManager _commandManager;

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public RelayCommand(ICommandBuilder? commandBuilder, Action methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(ICommandBuilder? commandBuilder, Action methodToExecute, Func<bool> canExecuteEvaluator)
            : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(IScarletCommandManager? commandManager, Action methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
        }

        public RelayCommand(IScarletCommandManager? commandManager, Action methodToExecute, Func<bool>? canExecute)
            : this(commandManager, methodToExecute)
        {
            _canExecute = canExecute ?? throw new ArgumentException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));
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
