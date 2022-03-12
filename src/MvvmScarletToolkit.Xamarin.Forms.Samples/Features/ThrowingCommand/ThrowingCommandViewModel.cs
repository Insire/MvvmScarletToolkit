using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class ThrowingCommandViewModel : ViewModelBase
    {
        public string Description { get; } = "This Implementation provides a command that can be run once at a time and can provide feedback to its current execution state. The CommandButton will switch to a cancel command during execution and back after that.";

        public ConcurrentCommandBase Command { get; }

        public ThrowingCommandViewModel()
            : base(new ScarletCommandBuilder(ScarletDispatcher.Default, ScarletCommandManager.Default, IgnoreExceptionHandler.Default, WeakReferenceMessenger.Default, ScarletExitService.Default, new ScarletWeakEventManager(), (lambda) => new BusyStack(lambda)))
        {
            Command = CommandBuilder
                .Create(CommandImpl)
                .WithSingleExecution("ThrowingCommand")
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();
        }

        private async Task CommandImpl(CancellationToken token)
        {
            await Task.Delay(3000, token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            throw new Exception("test");
        }
    }
}
