namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    internal sealed class TestDispatcher : IScarletDispatcher
    {
        public Task Invoke(Action? action, CancellationToken token)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }

        public Task<T?> Invoke<T>(Func<T>? action, CancellationToken token)
        {
            return action == null
                ? Task.FromResult(default(T))
                : Task.FromResult<T?>( action.Invoke());
        }
    }
}
