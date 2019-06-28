using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IWeakEventManager<TEventSource, TEventArgs>
         where TEventArgs : EventArgs
    {
        void AddHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler);

        void RemoveHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler);
    }
}
