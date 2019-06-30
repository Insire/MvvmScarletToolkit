using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public abstract class ConcurrentCommandDecoratorBase : ConcurrentCommandBase
    {
        protected readonly ConcurrentCommandBase Command;

        [Bindable(true, BindingDirection.OneWay)]
        public override bool IsBusy => Command.IsBusy;

        [Bindable(true, BindingDirection.OneWay)]
        public override Task Completion => Command.Completion;

        protected ConcurrentCommandDecoratorBase(IScarletCommandManager commandManager, ConcurrentCommandBase command)
            : base(commandManager)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Command.CanExecuteChanged += Command_CanExecuteChanged;
            Command.PropertyChanged += Command_PropertyChanged;
            CancelCommand = Command.CancelCommand;
        }

        private void Command_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        [DebuggerStepThrough]
        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }
    }
}
