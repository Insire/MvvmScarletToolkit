using System;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public sealed class ScarletWeakEventManager : IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private readonly WeakEventManager _weakEventManager;
        private static readonly Lazy<ScarletWeakEventManager> _default = new Lazy<ScarletWeakEventManager>(() => new ScarletWeakEventManager());

        public static IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> Default => _default.Value;

        public ScarletWeakEventManager()
        {
            _weakEventManager = new WeakEventManager();
        }

        public void AddHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            _weakEventManager.AddEventHandler(handler, eventName);
        }

        public void RemoveHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            _weakEventManager.RemoveEventHandler(handler, eventName);
        }
    }
}
