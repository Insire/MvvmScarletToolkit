using MvvmScarletToolkit.Abstractions;
using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly IScarletCommandManager _commandManager;

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public RelayCommand(ICommandBuilder commandBuilder, Action methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(ICommandBuilder commandBuilder, Action methodToExecute, Func<bool> canExecuteEvaluator)
                        : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(IScarletCommandManager commandManager, Action methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
        }

        public RelayCommand(IScarletCommandManager commandManager, Action methodToExecute, Func<bool> canExecuteEvaluator)
            : this(commandManager, methodToExecute)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentException($"{nameof(canExecuteEvaluator)} can't be empty.", nameof(canExecuteEvaluator));
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute is null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        private readonly IScarletCommandManager _commandManager;

        public RelayCommand(ICommandBuilder commandBuilder, Action<T> methodToExecute)
            : this(commandBuilder?.CommandManager, methodToExecute)
        {
        }

        public RelayCommand(ICommandBuilder commandBuilder, Action<T> methodToExecute, Func<T, bool> canExecuteEvaluator)
            : this(commandBuilder?.CommandManager, methodToExecute, canExecuteEvaluator)
        {
        }

        public RelayCommand(IScarletCommandManager commandManager, Action<T> execute)
        {
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _execute = execute ?? throw new ArgumentException($"{nameof(execute)} can't be empty.", nameof(execute));
        }

        public RelayCommand(IScarletCommandManager commandManager, Action<T> execute, Func<T, bool> canExecute)
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

            if (parameter is null && typeof(T).IsValueType)
            {
                return _canExecute.Invoke(default);
            }

            switch (parameter)
            {
                case null:
                case T _:
                    return _canExecute.Invoke((T)parameter);
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
            _execute((T)parameter);
        }
    }
}
