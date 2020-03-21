using System;
using System.Windows;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// When a WPF window is first shown, there is no actual focus.
    /// When this attached property is set on a e.g. window, it requests a traverse of the focus,
    /// thereby going to the control that has the lowest tab order.
    /// </summary>
    /// <example>
    /// In xaml
    /// <Window namespace:Focus.First="true"></window>
    /// </example>
    public static class Focus
    {
        public static readonly DependencyProperty FirstProperty = DependencyProperty.RegisterAttached(
                "First",
                typeof(bool),
                typeof(Focus),
                new PropertyMetadata(false, OnFirstChanged));

        /// <summary>Helper for getting <see cref="FirstProperty"/> from <paramref name="frameworkElement"/>.</summary>
        /// <param name="frameworkElement"><see cref="FrameworkElement"/> to read <see cref="FirstProperty"/> from.</param>
        /// <returns>First property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static bool GetFirst(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(FirstProperty);
        }

        /// <summary>Helper for setting <see cref="FirstProperty"/> on <paramref name="frameworkElement"/>.</summary>
        /// <param name="frameworkElement"><see cref="FrameworkElement"/> to set <see cref="FirstProperty"/> on.</param>
        /// <param name="value">First property value.</param>
        public static void SetFirst(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(FirstProperty, value);
        }

        private static void OnFirstChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (!(o is FrameworkElement element) || !(e.NewValue is bool enabled))
            {
                return;
            }

            if (enabled)
            {
                element.Loaded -= OnLoaded;
                element.Loaded += OnLoaded;
            }
            else
            {
                element.Loaded -= OnLoaded;
            }

            // will be fired each time the control is loaded. E.g. when a theme change occurs, this will trigger again
            // see: https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.loaded#remarks
            void OnLoaded(object _, EventArgs __)
            {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
