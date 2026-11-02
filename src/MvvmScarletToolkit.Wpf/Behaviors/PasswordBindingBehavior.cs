using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Behavior that enables binding for <see cref="PasswordBox.Password"/>
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
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:PasswordBindingBehavior ClearTextPassword="{Binding StringProperty}" />
    // </ i:Interaction.Behaviors>
    public sealed class PasswordBindingBehavior : Behavior<PasswordBox>
    {
        public string? ClearTextPassword
        {
            get => (string?)GetValue(ClearTextPasswordProperty);
            set => SetValue(ClearTextPasswordProperty, value);
        }

        /// <summary>Identifies the <see cref="ClearTextPassword"/> dependency property.</summary>
        public static readonly DependencyProperty ClearTextPasswordProperty = DependencyProperty.Register(
            nameof(ClearTextPassword),
            typeof(string),
            typeof(PasswordBindingBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnClearTextPasswordChanged));

        private bool _isUpdatingPassword;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is null)
            {
                return;
            }

            AssociatedObject.PasswordChanged += PasswordChanged;
            UpdatePassword();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword)
            {
                return;
            }

            SetCurrentValue(ClearTextPasswordProperty, AssociatedObject.Password);
        }

        private static void OnClearTextPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBindingBehavior behavior)
            {
                behavior.UpdatePassword();
            }
        }

        private void UpdatePassword()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            var password = ClearTextPassword ?? string.Empty;
            if (AssociatedObject.Password == password)
            {
                // also prevents moving the caret while the user types
                return;
            }

            try
            {
                _isUpdatingPassword = true;
                AssociatedObject.Password = password;
            }
            finally
            {
                _isUpdatingPassword = false;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= PasswordChanged;

            base.OnDetaching();
        }
    }
}
