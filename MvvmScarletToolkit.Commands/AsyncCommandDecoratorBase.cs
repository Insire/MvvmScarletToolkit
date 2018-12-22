using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    internal abstract class AsyncCommandDecoratorBase : IExtendedAsyncCommand
    {
        protected readonly IExtendedAsyncCommand Command;

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand CancelCommand => Command.CancelCommand;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy => Command.IsBusy;

        public Task Completion => Command.Completion;

        public event EventHandler CanExecuteChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncCommandDecoratorBase(IExtendedAsyncCommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Command.CanExecuteChanged += Command_CanExecuteChanged;
            Command.PropertyChanged += Command_PropertyChanged;
        }

        private void Command_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        [DebuggerStepThrough]
        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        public abstract Task ExecuteAsync(object parameter);

        public abstract bool CanExecute(object parameter);
    }
}
