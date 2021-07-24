using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public sealed class ScarletDispatcher : IScarletDispatcher
    {
        private static readonly Lazy<ScarletDispatcher> _default = new Lazy<ScarletDispatcher>(() => new ScarletDispatcher());

        public static IScarletDispatcher Default => _default.Value;

        public ScarletDispatcher()
        {
        }

        public Task Invoke(Action action, CancellationToken token)
        {
            return Task.FromResult(action);
        }

        public Task<T> Invoke<T>(Func<T> action, CancellationToken token)
        {
            return Task.FromResult(action.Invoke());
        }
    }
}
