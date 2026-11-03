using System.ComponentModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Commands
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal sealed class SequentialAsyncCommandDecorator : ConcurrentCommandDecoratorBase
    {
        internal const string DefaultRunningReason = "Already running";

        private readonly string? _runningReason;
        private string? _name;

        // generated on first access, since the name is only used for diagnostics
        public string Name => _name ??= Guid.NewGuid().ToString();

        public override string? DisabledReason => IsNotRunning() ? Command.DisabledReason : _runningReason;

        public SequentialAsyncCommandDecorator(in IScarletCommandManager commandManager, in ConcurrentCommandBase command, string? id)
            : this(commandManager, command, id, DefaultRunningReason)
        {
        }

        public SequentialAsyncCommandDecorator(in IScarletCommandManager commandManager, in ConcurrentCommandBase command, string? id, string? runningReason)
            : base(commandManager, command)
        {
            if (id is not null && id.Length > 0)
            {
                _name = id;
            }

            _runningReason = runningReason;
        }

        protected override void OnCommandPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnCommandPropertyChanged(e);

            // the running state is derived from Completion, which changes on start and is followed by IsBusy on completion
            if (e.PropertyName is nameof(Completion) or nameof(IsBusy))
            {
                OnPropertyChanged(nameof(DisabledReason));
            }
        }

        public override bool CanExecute(object? parameter)
        {
            return IsNotRunning() && Command.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            if (!IsNotRunning())
            {
#if DEBUG
                Debug.WriteLine($"[{Name}] Execute: SKIPPED");
#endif
                return;
            }

#if DEBUG
            Debug.WriteLine($"[{Name}] Execute: RUNNING");
#endif

            await Command
                    .ExecuteAsync(parameter)
                    .ConfigureAwait(false);

#if DEBUG
            Debug.WriteLine($"[{Name}] Execute: COMPLETE");
#endif
        }

        /// <summary>
        /// workaround for xamarin forms, which, queues multiple command executions from consecutive button taps,
        /// without invoking ICommand.CanExecute for each of them
        /// </summary>
        private bool IsNotRunning()
        {
            // a command without a completion has never been executed
            return Completion?.IsCompleted != false;
        }

        private string GetDebuggerDisplay()
        {
            return Name;
        }
    }
}
