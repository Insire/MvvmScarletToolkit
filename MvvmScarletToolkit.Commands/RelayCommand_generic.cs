using MvvmScarletToolkit.Abstractions;
using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public sealed class RelayCommand<TArgument> : ICommand
    {
        private readonly Action<TArgument> _execute;
        private readonly Func<TArgument, bool> _canExecute;
        private readonly IScarletCommandManager _commandManager;

        public RelayCommand(ICommandBuilder commandBuilder, Action<TArgument> methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(ICommandBuilder commandBuilder, Action<TArgument> methodToExecute, Func<TArgument, bool> canExecuteEvaluator)
            : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(IScarletCommandManager commandManager, Action<TArgument> execute)
        {
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _execute = execute ?? throw new ArgumentException($"{nameof(execute)} can't be empty.", nameof(execute));
        }

        public RelayCommand(IScarletCommandManager commandManager, Action<TArgument> execute, Func<TArgument, bool> canExecute)
                        : this(commandManager, execute)
        {
            _canExecute = canExecute ?? throw new ArgumentException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute is null)
            {
                return true;
            }

            if (parameter is null && typeof(TArgument).IsValueType)
            {
                return _canExecute.Invoke(default);
            }

            switch (parameter)
            {
                case null:
                case TArgument _:
                    return _canExecute.Invoke((TArgument)parameter);
            }

            return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((TArgument)parameter);
        }
    }
}
