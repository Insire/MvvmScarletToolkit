using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Tests
{
    public class TestProxy : IScarletMessageProxy
    {
        public IScarletMessage Message { get; private set; }

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            Message = message;
            subscription.Deliver(message);
        }
    }
}
