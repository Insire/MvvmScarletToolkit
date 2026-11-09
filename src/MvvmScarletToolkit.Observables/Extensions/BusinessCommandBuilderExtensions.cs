using MvvmScarletToolkit.Commands;

namespace MvvmScarletToolkit.Observables
{
    internal static class BusinessCommandBuilderExtensions
    {
        /// <summary>
        /// Builds a cancellable command that runs one execution at a time and reports itself on <paramref name="busyStack"/>
        /// </summary>
        public static ConcurrentCommandBase CreateBusinessCommand(this IScarletCommandBuilder commandBuilder, Func<CancellationToken, Task> execute, Func<bool> canExecute, IBusyStack busyStack)
        {
            return commandBuilder
                .Create(execute, canExecute)
                .WithSingleExecution()
                .WithBusyNotification(busyStack)
                .WithAsyncCancellation()
                .Build();
        }
    }
}
