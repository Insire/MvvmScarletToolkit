using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf
{
    public interface IToast : INotifyPropertyChanged
    {
        /// <summary>
        /// The title of the toast.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The message to display.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// will close and remove the toast after its configured display time
        /// </summary>
        ICommand BeginDismissCommand { get; }

        /// <summary>
        /// will close and remove the toast immediately
        /// </summary>
        ICommand DismissCommand { get; }

        /// <summary>
        /// If true, the toast will remain visible until the user closes it.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// This is set to true immediately when the alloted display time runs out, but the toast is not removed immediately from the toast collection,
        /// so that there is time to animate its removal
        /// </summary>
        bool IsRemoving { get; }

        /// <summary>
        /// The toast type to be displayed.
        /// </summary>
        Enum ToastType { get; }

        /// <summary>
        /// The duration to show the toast for.
        /// </summary>
        TimeSpan VisibleFor { get; }
    }
}
