using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class ScarletWeakEventManager : IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        public static IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> Default { get; } = new ScarletWeakEventManager();

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
