using System;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastServiceConfiguration
    {
        private const int DefaultIsVisibleForInSeconds = 3;

        /// <summary>
        /// the window style to look for
        /// </summary>
        public string WindowStyleKey { get; set; } = "DefaultToastWindowStyle";

        /// <summary>
        /// how far away the notification window is supposed to be from the screens edges
        /// </summary>
        public int WindowOffset { get; set; } = 12;

        /// <summary>
        /// how lojg to wait, before closing the notification window
        /// </summary>
        public TimeSpan WindowCloseDelay { get; set; } = TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds);

        /// <summary>
        /// how long to wait, before finally removing toasts from the toast collection
        /// </summary>
        public TimeSpan ToastCloseDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The container to display the toast within. Leave this as null to use the primary monitor.
        /// </summary>
        public Rect? Origin { get; set; }

        /// <summary>
        /// The duration to show the toast for.
        /// </summary>
        public TimeSpan ToastVisibleFor { get; set; } = TimeSpan.FromSeconds(DefaultIsVisibleForInSeconds);
    }
}
