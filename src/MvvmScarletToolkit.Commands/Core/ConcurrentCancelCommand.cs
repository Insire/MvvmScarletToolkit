using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class ConcurrentCancelCommand : GenericConcurrentCommandBase, ICancelCommand
    {
        private CancellationTokenSource? _cts;
        private readonly IScarletExceptionHandler _exceptionHandler;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ConcurrentCancelCommand(in IScarletCommandManager commandManager, in IScarletExceptionHandler exceptionHandler)
            : base(commandManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new System.ArgumentNullException(nameof(exceptionHandler));
        }

        public void NotifyCommandStarting()
        {
            IsBusy = true;
            if (_cts?.IsCancellationRequested == false)
            {
                return;
            }

            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Token));
        }

        public void NotifyCommandFinished()
        {
            IsBusy = false;
            RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }

        public override async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter)
                .ConfigureAwait(false);
        }

        public override bool CanExecute(object? parameter)
        {
            return IsBusy && _cts?.IsCancellationRequested != true;
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            Execution = new NotifyTaskCompletion(Task.Run(() =>
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }), _exceptionHandler);

            OnPropertyChanged(nameof(Completion));

            await Completion
                .ConfigureAwait(true); // return to UI thread here

            RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Token));
        }
    }
}
