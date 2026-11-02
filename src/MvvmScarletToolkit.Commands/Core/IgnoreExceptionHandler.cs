namespace MvvmScarletToolkit
{
    public sealed class IgnoreExceptionHandler : IScarletExceptionHandler
    {
        public static IScarletExceptionHandler Default { get; } = new IgnoreExceptionHandler();

        public Task Handle(Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif

            return Task.CompletedTask;
        }
    }
}
