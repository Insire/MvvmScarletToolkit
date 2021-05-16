using DynamicData.Binding;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf
{
    public interface IToastService : INotifyPropertyChanged, IDisposable
    {
        IObservableCollection<IToast> Items { get; }

        /// <summary>
        /// how long to wait, before finally removing toasts from the toast collection
        /// </summary>
        TimeSpan ToastCloseDelay { get; }

        /// <summary>
        /// Show a toast notification according to the service configuration
        /// </summary>
        void Show(IToast toast);

        /// <summary>
        /// will close and remove a toast immediately
        /// </summary>
        ICommand DismissCommand { get; }
    }
}
