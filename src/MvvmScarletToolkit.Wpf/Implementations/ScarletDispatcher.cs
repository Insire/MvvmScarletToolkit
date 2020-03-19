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

        internal bool InvokeSynchronous { get; set; }

        public ScarletDispatcher(Dispatcher dispatcher)
        {
            _dispatcherObject = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task Invoke(Action action, CancellationToken token)
        {
            if (action is null)
            {
                return;
            }

            if (InvokeSynchronous)
            {
                _dispatcherObject.Invoke(action, DispatcherPriority.Normal);
                return;
            }

            await _dispatcherObject.InvokeAsync(action, Priority, token);
        }

        public async Task<T> Invoke<T>(Func<T> action, CancellationToken token)
        {
            if (action is null)
            {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                return default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
            }

            if (InvokeSynchronous)
            {
                return _dispatcherObject.Invoke(action, DispatcherPriority.Normal);
            }

            return await _dispatcherObject.InvokeAsync(action, Priority, token);
        }
    }
}
