using MvvmScarletToolkit.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class ConcurrentCancelCommand : GenericConcurrentCommandBase, ICancelCommand
    {
        private CancellationTokenSource? _cts;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ConcurrentCancelCommand(IScarletCommandManager commandManager)
            : base(commandManager)
        {
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

        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        public override bool CanExecute(object parameter)
        {
            return IsBusy && _cts?.IsCancellationRequested != true;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            Execution = new NotifyTaskCompletion(Task.Run(() =>
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }));

            OnPropertyChanged(nameof(Completion));

            await Completion;

            RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Token));
        }
    }
}
