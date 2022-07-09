using System;

namespace MvvmScarletToolkit.Observables
{
    public struct BusyToken : IDisposable
    {
        private readonly IBusyStack _busyStack;

        private bool _disposed;

        public BusyToken(in IBusyStack busyStack)
        {
            _busyStack = busyStack ?? throw new ArgumentNullException(nameof(busyStack));
            _disposed = false;

            busyStack.Push();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _busyStack.Pull();
            _disposed = true;
        }
    }
}
