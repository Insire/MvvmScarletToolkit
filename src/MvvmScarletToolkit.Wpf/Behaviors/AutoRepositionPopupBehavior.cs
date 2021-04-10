using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Behavior that enables synchronizing a <see cref="Popup"/>s position, when the <see cref="Window"/> its attached to, is being moved
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:i="http://schemas.microsoft.com/xaml/behaviors"</description>
    /// </item>
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // source: https://putridparrot.com/blog/automatically-update-a-wpf-popup-position/
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:AutoRepositionPopupBehavior />
    // </ i:Interaction.Behaviors>
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif

    public sealed class AutoRepositionPopupBehavior : Behavior<Popup>
    {
        private const int WM_MOVING = 0x0216;

        private IDisposable? _disposableHandle;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.LayoutUpdated += AssociatedObject_LayoutUpdated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LayoutUpdated -= AssociatedObject_LayoutUpdated;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            _disposableHandle?.Dispose();

            base.OnDetaching();
        }

        private void AssociatedObject_LayoutUpdated(object? sender, EventArgs? e)
        {
            ForceUpdatePopupPosition();
        }

        private void AssociatedObject_Loaded(object? sender, RoutedEventArgs? e)
        {
            var root = AssociatedObject.PlacementTarget.FindParent<Window>();
            if (!(root is Window window))
            {
                return;
            }

            var helper = new WindowInteropHelper(window);
            var hwndSource = HwndSource.FromHwnd(helper.Handle);
            if (hwndSource is null)
            {
                return;
            }

            hwndSource.AddHook(HwndMessageHook);
            _disposableHandle = hwndSource;
        }

        private IntPtr HwndMessageHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            if (msg == WM_MOVING)
            {
                ForceUpdatePopupPosition();
            }

            return IntPtr.Zero;
        }

        private void ForceUpdatePopupPosition()
        {
            var offset = AssociatedObject.HorizontalOffset;

            AssociatedObject.SetCurrentValue(Popup.HorizontalOffsetProperty, offset + 1);
            AssociatedObject.SetCurrentValue(Popup.HorizontalOffsetProperty, offset);
        }
    }
}
