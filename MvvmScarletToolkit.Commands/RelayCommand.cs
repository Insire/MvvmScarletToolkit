using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
        }

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator) : this(methodToExecute)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentException($"{nameof(canExecuteEvaluator)} can't be empty.", nameof(canExecuteEvaluator));
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentException($"{nameof(execute)} can't be empty.", nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default);

            switch (parameter)
            {
                case null:
                case T _:
                    return (_canExecute.Invoke((T)parameter));
            }

            return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
