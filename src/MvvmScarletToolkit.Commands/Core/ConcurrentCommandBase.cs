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

        public event PropertyChangedEventHandler? PropertyChanged;

        [Bindable(true, BindingDirection.OneWay)]
        public abstract Task Completion { get; }

        private ICommand? _cancelCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand? CancelCommand
        {
            get { return _cancelCommand; }
            protected set { SetValue(ref _cancelCommand, value); }
        }

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
            CommandManager = commandManager ?? throw new ArgumentNullException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName]string? propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, T value, Action? OnChanged, [CallerMemberName]string? propertyName = null)
        {
            return SetValue(ref field, value, null, OnChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, T value, Action? OnChanging, Action? OnChanged, [CallerMemberName]string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            OnChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            OnChanged?.Invoke();

            return true;
        }
    }
}
