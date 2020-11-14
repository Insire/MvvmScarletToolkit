using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class SequentialAsyncCommandDecorator : ConcurrentCommandDecoratorBase
    {
        public SequentialAsyncCommandDecorator(in IScarletCommandManager commandManager, in ConcurrentCommandBase command)
            : base(commandManager, command)
        {
        }

        public override bool CanExecute(object parameter)
        {
            if (IsBusy)
            {
                return false;
            }

            if (Command.Completion?.IsCompleted == false)
            {
                return false;
            }

            return Command.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var shouldExecute = await ShouldExecute();

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
            var state = Command.Completion.Status;

            await Command.Completion.ConfigureAwait(false);

            return state != TaskStatus.Running
                && state != TaskStatus.WaitingForActivation
                && state != TaskStatus.WaitingToRun
                && state != TaskStatus.WaitingForChildrenToComplete;
        }
    }
}
