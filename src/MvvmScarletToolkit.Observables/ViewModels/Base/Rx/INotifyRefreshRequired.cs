using System;
using System.Reactive;

namespace MvvmScarletToolkit
{
    public interface INotifyRefreshRequired
    {
        IObservable<Unit> GetObservable();

        void Notify();
    }
}
