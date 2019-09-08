using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public sealed class PasswordBehavior : Behavior<PasswordBox>
    {
        public string ClearTextPassword
        {
            get { return (string)GetValue(ClearTextPasswordProperty); }
            set { SetValue(ClearTextPasswordProperty, value); }
        }

        /// <summary>Identifies the <see cref="ClearTextPassword"/> dependency property.</summary>
        public static readonly DependencyProperty ClearTextPasswordProperty = DependencyProperty.Register(
            nameof(ClearTextPassword),
            typeof(string),
            typeof(PasswordBehavior),
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
