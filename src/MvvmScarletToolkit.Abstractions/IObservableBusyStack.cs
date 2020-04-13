using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IObservableBusyStack : IObservable<bool>, IBusyStack, IDisposable
    {
    }
}
