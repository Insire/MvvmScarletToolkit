using MvvmScarletToolkit.Abstractions;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class SequentialAsyncCommandDecorator : ConcurrentCommandDecoratorBase
    {
        public SequentialAsyncCommandDecorator(IExtendedAsyncCommand command)
            : base(command)
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
            await Command.Completion.ConfigureAwait(false);
            await Command.ExecuteAsync(parameter).ConfigureAwait(false);
        }
    }
}
