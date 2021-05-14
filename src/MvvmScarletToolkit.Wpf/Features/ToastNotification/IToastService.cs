using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf
{
    public interface IToastService : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IToast> Items { get; }

        /// <summary>
        /// how long to wait, before finally removing toasts from the toast collection
        /// </summary>
        TimeSpan ToastCloseDelay { get; }

        /// <summary>
        /// Show a toast notification for a given time frame.
        /// </summary>
        Task Show(IToast toast);

        /// <summary>
        /// Remove a toast notification.
        /// </summary>
        Task Remove(IToast toast);
    }
}
