using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public class ClearPasswordBoxAction : TriggerAction<Button>
    {
        /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            nameof(Target),
            typeof(PasswordBox),
            typeof(ClearPasswordBoxAction),
            new UIPropertyMetadata(default(PasswordBox)));

        public PasswordBox? Target
        {
            get { return (PasswordBox?)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            Target?.Clear();
            Target?.SelectAll();
        }
    }
}
