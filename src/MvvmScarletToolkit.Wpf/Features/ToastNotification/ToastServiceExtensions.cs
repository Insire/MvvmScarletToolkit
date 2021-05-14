using System;
using System.Threading.Tasks;

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
        public static Task Show(this IToastService toastService, string title, string body, ToastType toastType)
        {
            return toastService.Show(title, body, toastType, TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds), false);
        }

        /// <summary>
        /// Show a toast notification for 5 seconds.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="toastType">The toast type to be displayed.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static Task Show(this IToastService toastService, string title, string body, ToastType toastType, bool isPersistent)
        {
            return toastService.Show(title, body, toastType, TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds), isPersistent);
        }

        /// <summary>
        /// Show a toast notification for a given time frame.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="body">The message to display.</param>
        /// <param name="type">The toast type to be displayed.</param>
        /// <param name="visibleFor">The duration to show the toast for.</param>
        /// <param name="isPersistent">If true, the toast will remain visible until the user closes it.</param>
        public static Task Show(this IToastService toastService, string title, string body, Enum type, TimeSpan visibleFor, bool isPersistent)
        {
            return toastService.Show(new ToastViewModel(toastService, title, body, type, isPersistent, visibleFor));
        }
    }
}
