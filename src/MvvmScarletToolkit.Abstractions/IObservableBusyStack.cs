using System;

namespace MvvmScarletToolkit
{
    public interface IObservableBusyStack : IObservable<bool>, IBusyStack, IDisposable;
}
