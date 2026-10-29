using System.Diagnostics;

namespace MvvmScarletToolkit.Commands
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal sealed class SequentialAsyncCommandDecorator : ConcurrentCommandDecoratorBase
    {
        private string? _name;

        // generated on first access, since the name is only used for diagnostics
        public string Name => _name ??= Guid.NewGuid().ToString();

        public SequentialAsyncCommandDecorator(in IScarletCommandManager commandManager, in ConcurrentCommandBase command, string? id)
            : base(commandManager, command)
        {
            if (id is not null && id.Length > 0)
            {
                _name = id;
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
