using System;

namespace MvvmScarletToolkit.Observables
{
    public struct BusyToken : IDisposable
    {
        private readonly BusyStack _busyStack;

        public BusyToken(BusyStack busyStack)
        {
            _busyStack = busyStack ?? throw new ArgumentNullException(nameof(busyStack));
            busyStack.Push(this);
        }

        public void Dispose()
        {
            _busyStack.Pull();
        }
    }
}
