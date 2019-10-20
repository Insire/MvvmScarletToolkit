using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public abstract class DialogResultViewModel<TResult> : ViewModelBase<ViewModelContainer<TResult>>
    {
        protected EventHandler DialogClosed;

        public ICommand CloseCommand { get; }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            protected set { SetValue(ref _isOpen, value, OnChanged: OnOpenChanged); }
        }

        protected DialogResultViewModel(ICommandBuilder commandBuilder, TResult model)
            : base(commandBuilder, new ViewModelContainer<TResult>(model))
        {
            CloseCommand = commandBuilder
                .Create(Close, CanClose)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();
        }

        public Task<TResult> Show()
        {
            return Show(CancellationToken.None);
        }

        public async Task<TResult> Show(CancellationToken token)
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
                    if (token.IsCancellationRequested && CanClose())
                    {
                        await Close().ConfigureAwait(false);
                    }

                    DialogClosed -= OnClosed;
                }

                void OnClosed(object sender, EventArgs e)
                {
                    tcs.TrySetResult(null);
                }
            }

            return Model.Value;
        }

        private void OnOpenChanged()
        {
            if (IsOpen)
            {
                return;
            }

            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private Task Close()
        {
            return Dispatcher.Invoke(() => IsOpen = false);
        }

        protected virtual bool CanClose()
        {
            return IsOpen;
        }
    }

    public abstract class DialogResultViewModel : ViewModelBase
    {
        protected EventHandler DialogClosed;

        public ICommand CloseCommand { get; }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            protected set { SetValue(ref _isOpen, value, OnChanged: OnOpenChanged); }
        }

        protected DialogResultViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            CloseCommand = commandBuilder
                .Create(Close, CanClose)
                .WithSingleExecution(CommandManager)
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
