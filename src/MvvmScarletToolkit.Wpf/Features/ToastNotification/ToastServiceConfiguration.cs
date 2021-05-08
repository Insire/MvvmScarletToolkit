using System;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastServiceConfiguration
    {
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
        public TimeSpan WindowCloseDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// how long to wait, before finally removing toasts from the toast collection
        /// </summary>
        public TimeSpan ToastCloseDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
}
