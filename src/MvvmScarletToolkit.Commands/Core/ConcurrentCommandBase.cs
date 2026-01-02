using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        public abstract Task? Completion { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand? CancelCommand
        {
            get { return field; }
            protected set { SetValue(ref field, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual bool IsBusy
        {
            get { return field; }
            protected set
            {
                if (SetValue(ref field, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                }
            }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsNotBusy => !IsBusy;

        public abstract void Execute(object? parameter);

        public abstract bool CanExecute(object? parameter);

        public abstract Task ExecuteAsync(object? parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected ConcurrentCommandBase(in IScarletCommandManager commandManager)
        {
            CommandManager = commandManager ?? throw new ArgumentNullException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        protected void OnPropertyChanged([CallerMemberName] in string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected bool SetValue<T>(ref T field, in T value, [CallerMemberName] in string? propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, in T value, in Action? onChanged, [CallerMemberName] in string? propertyName = null)
        {
            return SetValue(ref field, value, null, onChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, in T value, in Action? onChanging, in Action? onChanged, [CallerMemberName] in string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            onChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            onChanged?.Invoke();

            return true;
        }
    }
}
