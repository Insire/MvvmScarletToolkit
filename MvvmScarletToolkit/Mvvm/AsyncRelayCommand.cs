﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute = null;
        private readonly Func<object, Task> _execute = null;
        private Task _task;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<object, Task> methodToExecute)
        {
            if (methodToExecute == null)
                throw new ArgumentNullException(nameof(methodToExecute));

            _execute = methodToExecute;
        }

        public AsyncRelayCommand(Func<object, Task> methodToExecute, Func<bool> canExecuteEvaluator):this(methodToExecute)
        {
            if (canExecuteEvaluator == null)
                throw new ArgumentNullException(nameof(canExecuteEvaluator));

            _canExecute = canExecuteEvaluator;
        }
        

        public bool CanExecute(object parameter)
        {
            return _task == null || _task.IsCompleted;
        }

        public async void Execute(object parameter)
        {
            _task = _execute(parameter);

            await _task;
        }
    }
}
