using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MvvmScarletToolkit
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                //_synchronizationContext.Send(delegate { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }, null);
                _synchronizationContext.Send(state=> PropertyChanged(this, new PropertyChangedEventArgs(propertyName)), null);
            }
        }

        public bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        public void SetValue<T>(ref T field, T value, Action OnChanged, [CallerMemberName]string propertyName = null)
        {
            if (SetValue(ref field, value, propertyName))
                OnChanged();
        }
    }
}
