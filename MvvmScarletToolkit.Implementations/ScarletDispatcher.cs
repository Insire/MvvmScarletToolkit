using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        public async Task Invoke(Action action, CancellationToken token)
        {
            await _dispatcherObject.InvokeAsync(action, DispatcherPriority.Normal, token);
        }

        public async Task<T> Invoke<T>(Func<T> action, CancellationToken token)
        {
            return await _dispatcherObject.InvokeAsync(action, DispatcherPriority.Normal, token);
        }
    }
}
