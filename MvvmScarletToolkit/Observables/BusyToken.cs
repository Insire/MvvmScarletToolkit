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
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                var stack = Target as BusyStack;
                stack?.Pull();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
