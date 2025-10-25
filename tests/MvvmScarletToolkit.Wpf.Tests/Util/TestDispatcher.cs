using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests.Util
{
    internal sealed class TestDispatcher : IScarletDispatcher
    {
        public Task Invoke(Action action, CancellationToken token)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }

        public Task<T> Invoke<T>(Func<T> action, CancellationToken token)
        {
            return Task.FromResult(action.Invoke());
        }
    }
}
