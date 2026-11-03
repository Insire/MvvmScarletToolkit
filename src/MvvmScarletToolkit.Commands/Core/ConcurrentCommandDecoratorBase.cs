using System.ComponentModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Commands
{
    public abstract class ConcurrentCommandDecoratorBase : ConcurrentCommandBase
    {
        protected readonly ConcurrentCommandBase Command;

        [Bindable(true, BindingDirection.OneWay)]
        public override bool IsBusy => Command.IsBusy;

        [Bindable(true, BindingDirection.OneWay)]
        public override Task? Completion => Command.Completion;

        [Bindable(true, BindingDirection.OneWay)]
        public override string? DisabledReason => Command.DisabledReason;

        protected ConcurrentCommandDecoratorBase(in IScarletCommandManager commandManager, in ConcurrentCommandBase command)
            : base(commandManager)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Command.PropertyChanged += Command_PropertyChanged;
            CancelCommand = Command.CancelCommand;
        }

        private void Command_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCommandPropertyChanged(e);
        }

        /// <summary>
        /// Called when a property of the decorated command changed. Forwards the notification by default.
        /// </summary>
        protected virtual void OnCommandPropertyChanged(PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        [DebuggerStepThrough]
        public override sealed async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter)
                .ConfigureAwait(true); // return to UI thread here
        }
    }
}
