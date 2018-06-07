using System;

namespace MvvmScarletToolkit
{
    public sealed class BusyToken : WeakReference, IDisposable
    {
        public BusyToken(BusyStack stack)
             : base(stack)
        {
            stack?.Push(this);
        }
        private void DisposeInternal()
        {
            var stack = Target as BusyStack;
            stack?.Pull();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }
    }
}
