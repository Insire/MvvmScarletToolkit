using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BaseViewModel that provides the implementation for <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName]string? propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, T value, Action? onChanged, [CallerMemberName]string? propertyName = null)
        {
            return SetValue(ref field, value, null, onChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, T value, Action? onChanging, Action? onChanged, [CallerMemberName]string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            onChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            onChanged?.Invoke();

            return true;
        }

#if DEBUG

        protected void LogMethodCall<T>([CallerMemberName]string? methodName = null)
        {
            var type = typeof(T);
            var name = type.GetGenericTypeName();

            Debug.WriteLine($"{name}.{methodName}");
        }

#endif
    }
}
