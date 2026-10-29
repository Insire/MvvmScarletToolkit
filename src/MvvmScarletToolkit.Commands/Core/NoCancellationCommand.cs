namespace MvvmScarletToolkit.Commands
{
    internal sealed class NoCancellationCommand : ICancelCommand
    {
        public static NoCancellationCommand Default { get; } = new NoCancellationCommand();

        public CancellationToken Token => CancellationToken.None;

        // CanExecute never changes, so subscribers don't need to be tracked or notified
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        private NoCancellationCommand()
        {
        }

        public bool CanExecute(object? parameter)
        {
            return false;
        }

        public void Dispose()
        {
        }

        public void Execute(object? parameter)
        {
        }

        public void NotifyCommandFinished()
        {
        }

        public void NotifyCommandStarting()
        {
        }
    }
}
