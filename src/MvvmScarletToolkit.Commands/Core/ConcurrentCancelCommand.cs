namespace MvvmScarletToolkit.Commands
{
    internal sealed class ConcurrentCancelCommand : GenericConcurrentCommandBase, ICancelCommand
    {
        private CancellationTokenSource? _cts;
        private int _runningExecutions;
        private readonly IScarletExceptionHandler _exceptionHandler;

        public CancellationToken Token => Volatile.Read(ref _cts)?.Token ?? CancellationToken.None;

        public ConcurrentCancelCommand(in IScarletCommandManager commandManager, in IScarletExceptionHandler exceptionHandler)
            : base(commandManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public void NotifyCommandStarting()
        {
            // the owning command invalidates the requery state after this call, so there is no need to raise CanExecuteChanged here
            IsBusy = Interlocked.Increment(ref _runningExecutions) > 0;
            if (Volatile.Read(ref _cts) is not null)
            {
                return;
            }

            Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
            OnPropertyChanged(nameof(Token));
        }

        public void NotifyCommandFinished()
        {
            IsBusy = Interlocked.Decrement(ref _runningExecutions) > 0;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _cts, null)?.Dispose();
        }

        public override async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter)
                .ConfigureAwait(false);
        }

        public override bool CanExecute(object? parameter)
        {
            return IsBusy && Volatile.Read(ref _cts) is not null;
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            // detach the source synchronously, so that an execution starting right after this call receives a fresh token.
            // the source is not disposed, since running executions may still register callbacks on its token
            var cts = Interlocked.Exchange(ref _cts, null);
            if (cts is null)
            {
                return;
            }

            OnPropertyChanged(nameof(Token));
            RaiseCanExecuteChanged();

            // cancellation callbacks run synchronously, so keep them off the calling thread
            Execution = new NotifyTaskCompletion(Task.Run(cts.Cancel), _exceptionHandler);

            await Completion
                .ConfigureAwait(false);
        }
    }
}
