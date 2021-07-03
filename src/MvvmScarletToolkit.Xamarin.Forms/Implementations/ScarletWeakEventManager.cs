using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    public sealed class ScarletWeakEventManager : IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private static readonly Lazy<ScarletWeakEventManager> _default = new Lazy<ScarletWeakEventManager>(() => new ScarletWeakEventManager());

        public static IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> Default => _default.Value;

        private readonly WeakEventManager weakEventManager;

        public ScarletWeakEventManager()
        {
            weakEventManager = new WeakEventManager();
        }

        public void AddHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            weakEventManager.AddEventHandler(handler, eventName);
        }

        public void RemoveHandler(INotifyPropertyChanged source, string eventName, EventHandler<PropertyChangedEventArgs> handler)
        {
            weakEventManager.RemoveEventHandler(handler, eventName);
        }
    }
}
