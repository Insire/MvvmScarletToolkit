using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public sealed class ScarletDispatcher : IScarletDispatcher
    {
        public static IScarletDispatcher Default { get; } = new ScarletDispatcher(Application.Current.Dispatcher);

        private readonly Dispatcher _dispatcherObject;

        public ScarletDispatcher(Dispatcher dispatcher)
        {
            _dispatcherObject = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task Invoke(Action action)
        {
            await _dispatcherObject.InvokeAsync(action);
        }

        public async Task<T> Invoke<T>(Func<T> action)
        {
            return await _dispatcherObject.InvokeAsync(action);
        }
    }
}
