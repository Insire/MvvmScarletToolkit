using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MvvmScarletToolkit
{
    // source: https://putridparrot.com/blog/automatically-update-a-wpf-popup-position/
    public class AutoRepositionPopupBehavior : Behavior<Popup>
    {
        private const int WM_MOVING = 0x0216;

        private IDisposable? _disposableHandle;

        // should be moved to a helper class
        private static DependencyObject GetTopmostParent(DependencyObject element)
        {
            var current = element;
            var result = element;

            while (current != null)
            {
                result = current;
                current = (current is Visual || current is Visual3D) ?
                    VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

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
            var root = GetTopmostParent(AssociatedObject.PlacementTarget);
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
