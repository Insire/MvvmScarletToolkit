using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public abstract class DialogResultViewModel : ViewModelBase
    {
        protected EventHandler DialogClosed;

        public ICommand CloseCommand { get; }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            protected set
            {
                if (SetProperty(ref _isOpen, value))
                {
                    OnOpenChanged();
                }
            }
        }

        protected DialogResultViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            CloseCommand = commandBuilder
                .Create(Close, CanClose)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();
        }

        public Task Show()
        {
            return Show(CancellationToken.None);
        }

        public async Task Show(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<object>();
            using (var registration = token.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    DialogClosed += OnClosed;
                    await Dispatcher.Invoke(() => IsOpen = true).ConfigureAwait(false); // open dialog
                    await tcs.Task.ConfigureAwait(false); // wait for dialog to close
                }
                finally
                {
                    DialogClosed -= OnClosed;
                }

                void OnClosed(object sender, EventArgs e)
                {
                    tcs.TrySetResult(null);
                }
            }
        }

        private void OnOpenChanged()
        {
            if (IsOpen)
            {
                return;
            }

            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private Task Close(CancellationToken token)
        {
            return Dispatcher.Invoke(() => IsOpen = false);
        }

        protected virtual bool CanClose()
        {
            return IsOpen;
        }
    }
}
