using System;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public static class ToastServiceExtensions
    {
        /// <summary>
        /// Show an error toast notification for 5 seconds.
        /// </summary>
        /// <param name="error">The error to display. The content will be the Message property on the exception.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toaster, Exception error, Rect? origin = null, bool isPersistent = false)
        {
            toaster.Show("Operation Failed", error.Message, origin, isPersistent);
        }

        /// <summary>
        /// Show an error toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="error">The error to display. The content will be the Message property on the exception.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toaster, string title, Exception error, Rect? origin = null, bool isPersistent = false)
        {
            toaster.Show(title, error.Message, origin, isPersistent);
        }

        /// <summary>
        /// Show an error toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toaster, string title, string message, Rect? origin = null, bool isPersistent = false)
        {
            toaster.Show(title, message, ToastType.Error, origin, isPersistent);
        }

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toaster, string title, string message, ToastType toastType, Rect? origin = null, bool isPersistent = false)
        {
            toaster.Show(title, message, toastType, new TimeSpan(0, 0, 5), origin, isPersistent);
        }
    }
}
