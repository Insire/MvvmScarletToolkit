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
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif

    public sealed class PasswordBindingBehavior : Behavior<PasswordBox>
    {
        public string? ClearTextPassword
        {
            get { return (string?)GetValue(ClearTextPasswordProperty); }
            set { SetValue(ClearTextPasswordProperty, value); }
        }

        /// <summary>Identifies the <see cref="ClearTextPassword"/> dependency property.</summary>
        public static readonly DependencyProperty ClearTextPasswordProperty = DependencyProperty.Register(
            nameof(ClearTextPassword),
            typeof(string),
            typeof(PasswordBindingBehavior),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is null)
            {
                return;
            }

            AssociatedObject.PasswordChanged += PasswordChanged;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(ClearTextPasswordProperty, AssociatedObject.Password);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= PasswordChanged;

            base.OnDetaching();
        }
    }
}
