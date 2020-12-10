using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal sealed class SequentialAsyncCommandDecorator : ConcurrentCommandDecoratorBase
    {
        public string Name { get; }

        public SequentialAsyncCommandDecorator(in IScarletCommandManager commandManager, in ConcurrentCommandBase command, string? id)
            : base(commandManager, command)
        {
            if (id is null || id.Length == 0)
            {
                Name = Guid.NewGuid().ToString();
            }
            else
            {
                Name = id;
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (Completion?.IsCompleted == false)
            {
                return false;
            }

            return Command.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var shouldExecute = await ShouldExecute().ConfigureAwait(false);

            if (shouldExecute)
            {
                await Command
                        .ExecuteAsync(parameter)
                        .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// workaround for xamarin forms, which, queues multiple command executions from consecutive button taps,
        /// without invoking ICommand.CanExecute for each of them
        /// </summary>
        private async Task<bool> ShouldExecute()
        {
            var state = Completion.Status;

            await Completion.ConfigureAwait(false);

            return state != TaskStatus.Running
                && state != TaskStatus.WaitingForActivation
                && state != TaskStatus.WaitingToRun
                && state != TaskStatus.WaitingForChildrenToComplete;
        }

        private string GetDebuggerDisplay()
        {
            return Name;
        }
    }
}
