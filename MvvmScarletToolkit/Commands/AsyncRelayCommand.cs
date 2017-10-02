﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    [Obsolete]
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<bool> _canExecute = null;
        private readonly Func<T, Task> _execute = null;
        private Task _task;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<T, Task> methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentNullException(nameof(methodToExecute));
        }

        public AsyncRelayCommand(Func<T, Task> methodToExecute, Func<bool> canExecuteEvaluator)
            : this(methodToExecute)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        public bool CanExecute(object parameter)
        {
            return _task == null && _canExecute();
        }

        public async void Execute(object parameter)
        {
            _task = _execute((T)parameter);

            try
            {
                await _task.ConfigureAwait(true);
            }
            finally
            {
                _task = null;
            }
        }
    }

    [Obsolete]
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute = null;
        private readonly Func<Task> _execute = null;
        private Task _task;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<Task> methodToExecute)
        {
            _execute = methodToExecute ?? throw new ArgumentNullException(nameof(methodToExecute));
        }

        public AsyncRelayCommand(Func<Task> methodToExecute, Func<bool> canExecuteEvaluator)
            : this(methodToExecute)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        public bool CanExecute(object parameter)
        {
            return _task == null && _canExecute();
        }

        public async void Execute(object parameter)
        {
            _task = _execute();

            try
            {
                await _task.ConfigureAwait(true);
            }
            finally
            {
                _task = null;
            }
        }
    }
}
