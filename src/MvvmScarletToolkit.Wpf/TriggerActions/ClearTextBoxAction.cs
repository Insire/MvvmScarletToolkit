using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public class ClearTextBoxAction : TriggerAction<Button>
    {
        /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            nameof(Target),
            typeof(TextBox),
            typeof(ClearTextBoxAction),
            new UIPropertyMetadata(default(TextBox)));

        public TextBox? Target
        {
            get { return (TextBox?)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            Target?.SetCurrentValue(TextBox.TextProperty, string.Empty);
            Target?.SelectAll();
        }
    }
}
