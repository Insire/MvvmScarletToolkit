using System;
using System.ComponentModel;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class ScarletWeakEventManager : IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private static readonly Lazy<ScarletWeakEventManager> _default = new Lazy<ScarletWeakEventManager>(() => new ScarletWeakEventManager());

        public static IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> Default => _default.Value;

        public ScarletWeakEventManager()
        {
        }

        public void AddHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(source, eventName, handler);
        }

        public void RemoveHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(source, eventName, handler);
        }
    }
}
