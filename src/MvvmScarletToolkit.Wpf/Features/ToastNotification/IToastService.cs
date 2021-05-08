using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public interface IToastService : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<ToastViewModel> Items { get; }

        /// <summary>
        /// Show a toast notification for a given time frame.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="type">The toast type to be displayed.</param>
        /// <param name="visibleFor">The duration to show the toast for.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        Task Show(string title, string body, Enum type, TimeSpan visibleFor, Rect? origin, bool isPersistent);
    }
}
