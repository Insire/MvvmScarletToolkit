using System;
using System.Reactive;
using System.Reactive.Linq;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Adapter class around <see cref="Observable.FromEventPattern"/>
    /// </summary>
    public sealed class NotifyRefreshRequiredAdapter : INotifyRefreshRequired
    {
        private event EventHandler<Unit>? RefreshRequired;

        public void Notify()
        {
            RefreshRequired?.Invoke(this, Unit.Default);
        }

        public IObservable<Unit> GetObservable()
        {
            return Observable
                .FromEventPattern<EventHandler<Unit>, Unit>(e => RefreshRequired += e, e => RefreshRequired -= e)
                .Select(x => x.EventArgs);
        }
    }
}
