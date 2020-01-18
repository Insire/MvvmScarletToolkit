using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Tests
{
    public class ExceptionHandlingProxy : IScarletMessageProxy
    {
        private readonly Action<Action> _exceptionHandler;

        public ExceptionHandlingProxy(Action<Action> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public IScarletMessage Message { get; private set; }

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            Message = message;

            _exceptionHandler.Invoke(() => subscription.Deliver(message));
        }
    }
}
