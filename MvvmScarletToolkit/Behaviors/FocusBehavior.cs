using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// When a WPF form is first shown, focus is a tricksy thing.  It's not actually anywhere.
    /// When this attached property is set on the form, it requests a traverse of the focus,
    /// thereby going to the control that has the lowest tab order.
    /// </summary>
    /// <example>
    /// In View xaml
    /// <Window namespace:FocusBehavior.FocusFirst="true"></window>
    /// </example>
    public static class FocusBehavior
    {
        public static readonly DependencyProperty FocusFirstProperty = DependencyProperty.RegisterAttached(
                "FocusFirst",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnFocusFirstChanged));

        /// <summary>Helper for getting <see cref="FocusFirstProperty"/> from <paramref name="control"/>.</summary>
        /// <param name="control"><see cref="Control"/> to read <see cref="FocusFirstProperty"/> from.</param>
        /// <returns>FocusFirst property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Control))]
        public static bool GetFocusFirst(Control control)
        {
            return (bool)control.GetValue(FocusFirstProperty);
        }

        /// <summary>Helper for setting <see cref="FocusFirstProperty"/> on <paramref name="control"/>.</summary>
        /// <param name="control"><see cref="Control"/> to set <see cref="FocusFirstProperty"/> on.</param>
        /// <param name="value">FocusFirst property value.</param>
        public static void SetFocusFirst(Control control, bool value)
        {
            control.SetValue(FocusFirstProperty, value);
        }

        private static void OnFocusFirstChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (!(o is Control control) || !(e.NewValue is bool flag))
            {
                return;
            }

            if (flag)
            {
                control.Loaded += (sender, _) => control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
