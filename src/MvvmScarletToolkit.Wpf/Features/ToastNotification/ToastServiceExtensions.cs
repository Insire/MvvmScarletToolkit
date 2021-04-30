using System;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public static class ToastServiceExtensions
    {
        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        public static void Show(this IToastService toaster, string title, string message, ToastType toastType)
        {
            toaster.Show(title, message, toastType, TimeSpan.FromSeconds(5), null, false);
        }

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toaster, string title, string message, ToastType toastType, Rect? origin, bool isPersistent)
        {
            toaster.Show(title, message, toastType, TimeSpan.FromSeconds(5), origin, isPersistent);
        }

        /// <summary>
        /// Show an error toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void ShowError(this IToastService toaster, string title, string message, Rect? origin, bool isPersistent)
        {
            toaster.Show(title, message, ToastType.Error, origin, isPersistent);
        }

        /// <summary>
        /// Show an error toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message to display.</param>
        public static void ShowError(this IToastService toaster, string title, string message)
        {
            toaster.Show(title, message, ToastType.Error, null, false);
        }
    }
}
