using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ClearActionTests : TraceTestBase
    {
        private static async Task<Button> Click(UIElement target, Microsoft.Xaml.Behaviors.TriggerAction action, Func<Button, Task> assert)
        {
            var button = new Button();
            var trigger = new Microsoft.Xaml.Behaviors.EventTrigger(nameof(Button.Click));
            trigger.Actions.Add(action);
            Interaction.GetTriggers(button).Add(trigger);

            var window = Ui.Show(new StackPanel { Children = { target, button } });
            await Ui.Settle();

            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            await assert(button);

            window.Close();

            return button;
        }

        [Fact]
        public Task ClearTextBoxAction_Should_Clear_Its_Target()
        {
            return StaRunner.Run(async () =>
            {
                var textBox = new TextBox { Text = "text" };

                await Click(textBox, new ClearTextBoxAction { Target = textBox }, _ =>
                {
                    Assert.Empty(textBox.Text);
                    return Task.CompletedTask;
                });
            });
        }

        [Fact]
        public Task ClearTextBoxAction_Should_Ignore_A_Missing_Target()
        {
            return StaRunner.Run(async () =>
            {
                var textBox = new TextBox { Text = "text" };

                await Click(textBox, new ClearTextBoxAction(), _ =>
                {
                    Assert.Equal("text", textBox.Text);
                    return Task.CompletedTask;
                });
            });
        }

        [Fact]
        public Task ClearPasswordBoxAction_Should_Clear_Its_Target()
        {
            return StaRunner.Run(async () =>
            {
                var passwordBox = new PasswordBox { Password = "secret" };

                await Click(passwordBox, new ClearPasswordBoxAction { Target = passwordBox }, _ =>
                {
                    Assert.Empty(passwordBox.Password);
                    return Task.CompletedTask;
                });
            });
        }

        [Fact]
        public Task ClearPasswordBoxAction_Should_Ignore_A_Missing_Target()
        {
            return StaRunner.Run(async () =>
            {
                var passwordBox = new PasswordBox { Password = "secret" };

                await Click(passwordBox, new ClearPasswordBoxAction(), _ =>
                {
                    Assert.Equal("secret", passwordBox.Password);
                    return Task.CompletedTask;
                });
            });
        }
    }
}
