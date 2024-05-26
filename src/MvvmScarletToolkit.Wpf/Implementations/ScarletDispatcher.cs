using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    [Bindable(false)]
    public sealed class ScarletDispatcher : IScarletDispatcher
    {
        private static readonly Lazy<ScarletDispatcher> _default = new Lazy<ScarletDispatcher>(() => new ScarletDispatcher(Application.Current.Dispatcher));

        public static IScarletDispatcher Default => InternalDefault;
        internal static ScarletDispatcher InternalDefault => _default.Value;

        private const DispatcherPriority Priority = DispatcherPriority.Input;

        private readonly Dispatcher _dispatcherObject;

        private bool invokeSynchronous;

        public ScarletDispatcher(Dispatcher dispatcher)
        {
            _dispatcherObject = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        internal bool GetInvokeSynchronous()
        {
            return invokeSynchronous;
        }

        internal void SetInvokeSynchronous(bool value)
        {
            invokeSynchronous = value;
        }

        public async Task Invoke(Action action, CancellationToken token)
        {
            if (action is null)
            {
                return;
            }

            if (GetInvokeSynchronous())
            {
                _dispatcherObject.Invoke(action, DispatcherPriority.Normal, token);
                return;
            }

            await _dispatcherObject.InvokeAsync(action, Priority, token);
        }

        public async Task<T> Invoke<T>(Func<T> action, CancellationToken token)
        {
            if (action is null)
            {
                return default!;
            }

            if (GetInvokeSynchronous())
            {
                return _dispatcherObject.Invoke(action, DispatcherPriority.Normal);
            }

            return await _dispatcherObject.InvokeAsync(action, Priority, token);
        }
    }
}
