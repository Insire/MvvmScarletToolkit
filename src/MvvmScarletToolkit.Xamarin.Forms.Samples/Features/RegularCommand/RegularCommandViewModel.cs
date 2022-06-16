using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class RegularCommandViewModel : ViewModelBase
    {
        public string Description { get; } = "This Implementation provides a command that can be run once at a time and can provide feedback to its current execution state. The CommandButton won't be disabled during execution.";
        public ConcurrentCommandBase Command { get; }

        public RegularCommandViewModel()
            : base(new ScarletCommandBuilder(ScarletDispatcher.Default, ScarletCommandManager.Default, IgnoreExceptionHandler.Default, WeakReferenceMessenger.Default, ScarletExitService.Default, new ScarletWeakEventManager(), (lambda) => new BusyStack(lambda)))
        {
            Command = CommandBuilder
                .Create(CommandImpl, CanImpl)
                .WithSingleExecution("RegularCommand")
                .WithBusyNotification(BusyStack)
                .Build();
        }

        private async Task CommandImpl(CancellationToken token)
        {
            await Task.Delay(2000, token).ConfigureAwait(false);
        }

        private bool CanImpl()
        {
            return true;
        }
    }
}
