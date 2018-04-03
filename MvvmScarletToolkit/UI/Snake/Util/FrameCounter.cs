using System;
using System.Threading;

namespace MvvmScarletToolkit
{
    internal sealed class FrameCounter : IDisposable
    {
        private readonly TimerCallback _callback;

        private Timer _timer;
        private int _counter = 0;
        private bool disposed = false;

        public FrameCounter(TimerCallback fpsUpdate)
        {
            _callback = fpsUpdate ?? throw new ArgumentNullException(nameof(fpsUpdate));
            _timer = new Timer(InvokeAndReset, null, 0, 1000);
        }

        public void UpdateCounter()
        {
            _counter++;
        }

        private void InvokeAndReset(object count)
        {
            _callback.Invoke(_counter);
            _counter = 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
