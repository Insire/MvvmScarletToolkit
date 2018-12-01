using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public sealed class ScarletDispatcher : IScarletDispatcher
    {
        private readonly Dispatcher _dispatcherObject;

        public ScarletDispatcher()
        {
            _dispatcherObject = System.Windows.Application.Current.Dispatcher;
        }

        public async Task Invoke(Action action)
        {
            await _dispatcherObject.InvokeAsync(action);
        }

        public async Task<T> Invoke<T>(Func<T> action)
        {
            return await _dispatcherObject.InvokeAsync<T>(action);
        }
    }
}
