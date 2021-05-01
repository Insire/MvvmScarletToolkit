using System;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public static class ToastServiceExtensions
    {
        private const int DefaultIsVisibleForInSeconds = 5;

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        public static Task Show(this IToastService toaster, string title, string body, ToastType toastType)
        {
            return toaster.Show(title, body, toastType, TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds), null, false);
        }

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static Task Show(this IToastService toaster, string title, string body, ToastType toastType, bool isPersistent)
        {
            return toaster.Show(title, body, toastType, TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds), null, isPersistent);
        }

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        /// <param name="origin">The container to display the toast within. Leave this as null to use the primary monitor.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static Task Show(this IToastService toaster, string title, string body, ToastType toastType, Rect? origin, bool isPersistent)
        {
            return toaster.Show(title, body, toastType, TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds), origin, isPersistent);
        }
    }
}
