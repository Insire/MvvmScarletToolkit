using System;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    public sealed class BusyToken : WeakReference, IDisposable
    {
        [DebuggerStepThrough]
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
