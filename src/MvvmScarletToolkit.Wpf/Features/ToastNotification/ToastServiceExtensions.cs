using System;

namespace MvvmScarletToolkit.Wpf
{
    public static class ToastServiceExtensions
    {
        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        public static void Show(this IToastService toastService, string title, string body, ToastType toastType)
        {
            toastService.Show(title, body, toastType, false);
        }

        /// <summary>
        /// Show a toast notification for a given time frame.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="type">The toast type to be displayed.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static void Show(this IToastService toastService, string title, string body, Enum type, bool isPersistent)
        {
            toastService.Show(new ToastViewModel(title, body, type, isPersistent));
        }
    }
}
