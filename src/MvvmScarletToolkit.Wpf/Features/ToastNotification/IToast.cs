using System;
using System.ComponentModel;

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
        /// If true, the toast will remain visible until the user closes it.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// This is set to true immediately when the alloted display time runs out, but the toast is not removed immediately from the toast collection,
        /// so that there is time to animate its removal
        /// </summary>
        bool IsRemoving { get; set; }

        /// <summary>
        /// The toast type to be displayed.
        /// </summary>
        Enum ToastType { get; }
    }
}
