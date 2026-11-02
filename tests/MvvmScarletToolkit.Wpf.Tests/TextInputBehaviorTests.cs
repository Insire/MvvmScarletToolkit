using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class AllowableCharactersTextBoxBehaviorTests : TraceTestBase
    {
        private static TextBox CreateTextBox(string text, int maxLength)
        {
            var textBox = new TextBox { Text = text };
            Interaction.GetBehaviors(textBox).Add(new AllowableCharactersTextBoxBehavior { MaxLength = maxLength });

            return textBox;
        }

        private static TextCompositionEventArgs Type(TextBox textBox, string text)
        {
            var args = new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, textBox, text))
            {
                RoutedEvent = UIElement.PreviewTextInputEvent,
            };
            textBox.RaiseEvent(args);

            return args;
        }

        private static DataObjectPastingEventArgs Paste(TextBox textBox, string text)
        {
            var args = new DataObjectPastingEventArgs(new DataObject(DataFormats.Text, text), false, DataFormats.Text)
            {
                RoutedEvent = DataObject.PastingEvent,
            };
            textBox.RaiseEvent(args);

            return args;
        }

        [Fact]
        public Task Typing_Over_A_Selection_Should_Replace_It()
        {
            return StaRunner.Run(() =>
            {
                var textBox = CreateTextBox("abcd", 4);
                textBox.Select(1, 3);

                Assert.False(Type(textBox, "x").Handled);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Typing_Beyond_MaxLength_Should_Be_Rejected()
        {
            return StaRunner.Run(() =>
            {
                var textBox = CreateTextBox("abcd", 4);
                textBox.Select(4, 0);

                Assert.True(Type(textBox, "x").Handled);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Pasting_Over_A_Selection_Should_Replace_It()
        {
            return StaRunner.Run(() =>
            {
                var textBox = CreateTextBox("abcd", 4);
                textBox.Select(1, 3);

                Assert.False(Paste(textBox, "xyz").CommandCancelled);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Pasting_Beyond_MaxLength_Should_Be_Cancelled()
        {
            return StaRunner.Run(() =>
            {
                var textBox = CreateTextBox("abcd", 4);
                textBox.Select(1, 3);

                Assert.True(Paste(textBox, "wxyz").CommandCancelled);

                return Task.CompletedTask;
            });
        }
    }

    public sealed class PasswordBindingBehaviorTests : TraceTestBase
    {
        [Fact]
        public Task Should_Apply_The_Value_Set_Before_Attaching()
        {
            return StaRunner.Run(() =>
            {
                var passwordBox = new PasswordBox();
                Interaction.GetBehaviors(passwordBox).Add(new PasswordBindingBehavior { ClearTextPassword = "initial" });

                Assert.Equal("initial", passwordBox.Password);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Update_PasswordBox_When_Value_Changes()
        {
            return StaRunner.Run(() =>
            {
                var passwordBox = new PasswordBox();
                var behavior = new PasswordBindingBehavior();
                Interaction.GetBehaviors(passwordBox).Add(behavior);

                behavior.ClearTextPassword = "changed";
                Assert.Equal("changed", passwordBox.Password);

                behavior.ClearTextPassword = null;
                Assert.Equal(string.Empty, passwordBox.Password);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Update_Value_When_PasswordBox_Changes()
        {
            return StaRunner.Run(() =>
            {
                var passwordBox = new PasswordBox();
                var behavior = new PasswordBindingBehavior();
                Interaction.GetBehaviors(passwordBox).Add(behavior);

                passwordBox.Password = "typed";

                Assert.Equal("typed", behavior.ClearTextPassword);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Update_The_Bound_Source_Both_Ways()
        {
            return StaRunner.Run(() =>
            {
                var viewModel = new TextViewModel { Text = "from view model" };
                var passwordBox = new PasswordBox();
                var behavior = new PasswordBindingBehavior();
                BindingOperations.SetBinding(behavior, PasswordBindingBehavior.ClearTextPasswordProperty, new Binding(nameof(TextViewModel.Text)) { Source = viewModel });
                Interaction.GetBehaviors(passwordBox).Add(behavior);

                Assert.Equal("from view model", passwordBox.Password);

                passwordBox.Password = "from view";
                Assert.Equal("from view", viewModel.Text);

                return Task.CompletedTask;
            });
        }
    }

    public sealed class TextViewModel
    {
        public string? Text { get; set; }
    }

    public sealed class WatermarkBehaviorTests : TraceTestBase
    {
        private static Adorner? GetWatermark(TextBox textBox)
        {
            return AdornerLayer.GetAdornerLayer(textBox)?.GetAdorners(textBox)?.SingleOrDefault(p => p.GetType().Name == "WatermarkAdorner");
        }

        private static string? GetWatermarkText(Adorner adorner)
        {
            return ((TextBlock)VisualTreeHelper.GetChild(adorner, 0)).Text;
        }

        [Fact]
        public Task Should_Not_Write_The_Watermark_Into_The_Bound_Text()
        {
            return StaRunner.Run(async () =>
            {
                var viewModel = new TextViewModel();
                var textBox = new TextBox();
                textBox.SetBinding(TextBox.TextProperty, new Binding(nameof(TextViewModel.Text)) { Source = viewModel, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                Interaction.GetBehaviors(textBox).Add(new WatermarkBehavior { Watermark = "Search" });

                var window = Ui.Show(textBox);
                await Ui.Settle();

                Assert.Multiple(() =>
                {
                    Assert.Null(viewModel.Text);
                    Assert.Equal(string.Empty, textBox.Text);
                });

                textBox.Text = "abc";
                textBox.Text = string.Empty;
                await Ui.Settle();

                Assert.Equal(string.Empty, viewModel.Text);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Show_Watermark_Only_While_Empty()
        {
            return StaRunner.Run(async () =>
            {
                var textBox = new TextBox();
                Interaction.GetBehaviors(textBox).Add(new WatermarkBehavior { Watermark = "Search" });

                var window = Ui.Show(textBox);
                await Ui.Settle();

                var watermark = GetWatermark(textBox);
                Assert.NotNull(watermark);
                Assert.Equal(Visibility.Visible, watermark.Visibility);
                Assert.Equal("Search", GetWatermarkText(watermark));

                textBox.Text = "abc";
                Assert.Equal(Visibility.Collapsed, watermark.Visibility);

                textBox.Text = string.Empty;
                Assert.Equal(Visibility.Visible, watermark.Visibility);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Update_Watermark_Text()
        {
            return StaRunner.Run(async () =>
            {
                var textBox = new TextBox();
                var behavior = new WatermarkBehavior { Watermark = "Search" };
                Interaction.GetBehaviors(textBox).Add(behavior);

                var window = Ui.Show(textBox);
                await Ui.Settle();

                behavior.Watermark = "Filter";

                Assert.Equal("Filter", GetWatermarkText(GetWatermark(textBox)!));
                window.Close();
            });
        }

        [Fact]
        public Task Should_Switch_Between_Watermark_And_Original_Style()
        {
            return StaRunner.Run(async () =>
            {
                var original = new Style(typeof(TextBox));
                var watermarkStyle = new Style(typeof(TextBox));
                var textBox = new TextBox { Style = original };
                Interaction.GetBehaviors(textBox).Add(new WatermarkBehavior { Watermark = "Search", WatermarkStyle = watermarkStyle });

                var window = Ui.Show(textBox);
                await Ui.Settle();

                Assert.Same(watermarkStyle, textBox.Style);

                textBox.Text = "abc";
                Assert.Same(original, textBox.Style);

                textBox.Text = string.Empty;
                Assert.Same(watermarkStyle, textBox.Style);
                window.Close();
            });
        }

        [Fact]
        public Task Detaching_Should_Remove_Watermark_And_Restore_Style()
        {
            return StaRunner.Run(async () =>
            {
                var original = new Style(typeof(TextBox));
                var textBox = new TextBox { Style = original };
                var behavior = new WatermarkBehavior { Watermark = "Search", WatermarkStyle = new Style(typeof(TextBox)) };
                Interaction.GetBehaviors(textBox).Add(behavior);

                var window = Ui.Show(textBox);
                await Ui.Settle();

                Interaction.GetBehaviors(textBox).Remove(behavior);

                Assert.Multiple(() =>
                {
                    Assert.Null(GetWatermark(textBox));
                    Assert.Same(original, textBox.Style);
                });
                window.Close();
            });
        }
    }
}
