using System;

namespace MvvmScarletToolkit
{
    public interface ISubscriberErrorHandler
    {
        void Handle(IScarletMessage message, Exception exception);
    }
}
