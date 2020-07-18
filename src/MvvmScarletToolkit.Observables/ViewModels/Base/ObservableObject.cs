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

        private string _typeName = string.Empty;

        protected virtual void OnPropertyChanged([CallerMemberName] in string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(in PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected bool SetValue<T>(ref T field, in T value, [CallerMemberName] in string? propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, in T value, in Action? onChanged, [CallerMemberName] in string? propertyName = null)
        {
            return SetValue(ref field, value, null, onChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, in T value, in Action? onChanging, in Action? onChanged, [CallerMemberName] in string? propertyName = null)
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

        protected void LogMethodCall<T>([CallerMemberName] in string? methodName = null)
        {
            if (_typeName.Length == 0)
            {
                var type = typeof(T);
                _typeName = type.GetGenericTypeName();
            }

            Debug.WriteLine($"{_typeName}.{methodName}");
        }

#endif
    }
}
