using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// base implementation for running commands in an async fashion and providing UI notifications
    /// </summary>
    public abstract class ConcurrentCommandBase : IConcurrentCommand
    {
        protected readonly IScarletCommandManager CommandManager;

        public event PropertyChangedEventHandler PropertyChanged;

        [Bindable(true, BindingDirection.OneWay)]
        public abstract Task Completion { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand CancelCommand { get; protected set; }

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        public abstract void Execute(object parameter);

        public abstract bool CanExecute(object parameter);

        public abstract Task ExecuteAsync(object parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected ConcurrentCommandBase(IScarletCommandManager commandManager)
        {
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
