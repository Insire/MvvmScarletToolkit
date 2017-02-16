using System;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute = null;
        private readonly Func<bool> _canExecute = null;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action methodToExecute)
        {
            if (methodToExecute == null)
                throw new ArgumentException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));

            _execute = methodToExecute;
        }

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator) : this(methodToExecute)
        {
            if(canExecuteEvaluator== null)
                throw new ArgumentException($"{nameof(canExecuteEvaluator)} can't be empty.", nameof(canExecuteEvaluator));

            _canExecute = canExecuteEvaluator;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentException($"{nameof(execute)} can't be empty.", nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute.Invoke(default(T));

            if (parameter == null || parameter is T)
                return (_canExecute.Invoke((T)parameter));

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
