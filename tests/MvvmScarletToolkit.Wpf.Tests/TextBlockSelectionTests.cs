using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class TextBlockSelectionTests : TraceTestBase
    {
        private static TextBlockSelectionBehavior Enable(TextBlock textBlock)
        {
            TextBlockSelection.SetIsEnabled(textBlock, true);

            return Interaction.GetBehaviors(textBlock).OfType<TextBlockSelectionBehavior>().Single();
        }

        private static TextPointer PositionOf(Run run, int offset) => run.ContentStart.GetPositionAtOffset(offset)!;

        private static void Layout(FrameworkElement element, double width = 400)
        {
            element.Measure(new Size(width, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();
        }

        [Fact]
        public Task Should_Start_Without_Selection()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                Enable(textBlock);

                Assert.Empty(TextBlockSelection.GetSelectedText(textBlock));
                Assert.False(ApplicationCommands.Copy.CanExecute(null, textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Select_All_Text()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                Enable(textBlock);

                ApplicationCommands.SelectAll.Execute(null, textBlock);

                Assert.Equal("some text", TextBlockSelection.GetSelectedText(textBlock));
                Assert.True(ApplicationCommands.Copy.CanExecute(null, textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Work_On_Derived_TextBlocks()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "plain <b>bold</b>" };

                Enable(textBlock).SelectAll();

                Assert.Equal("plain bold", TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Be_Enabled_From_A_Style()
        {
            return StaRunner.Run(() =>
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlockSelection.IsEnabledProperty, true));

                var textBlock = new TextBlock { Style = style };

                Assert.Single(Interaction.GetBehaviors(textBlock).OfType<TextBlockSelectionBehavior>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Not_Add_A_Second_Behavior()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock();
                Interaction.GetBehaviors(textBlock).Add(new TextBlockSelectionBehavior());

                TextBlockSelection.SetIsEnabled(textBlock, true);

                Assert.Single(Interaction.GetBehaviors(textBlock).OfType<TextBlockSelectionBehavior>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Restore_The_TextBlock_When_Disabled()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                var focusVisualStyle = textBlock.FocusVisualStyle;
                Enable(textBlock).SelectAll();

                TextBlockSelection.SetIsEnabled(textBlock, false);

                Assert.Empty(Interaction.GetBehaviors(textBlock));
                Assert.Empty(TextBlockSelection.GetSelectedText(textBlock));
                Assert.Empty(textBlock.CommandBindings);
                Assert.False(textBlock.Focusable);
                Assert.True(KeyboardNavigation.GetIsTabStop(textBlock));
                Assert.Same(focusVisualStyle, textBlock.FocusVisualStyle);
                Assert.Null(textBlock.Cursor);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Keep_A_Custom_Cursor()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Cursor = Cursors.Hand };

                Enable(textBlock);

                Assert.Same(Cursors.Hand, textBlock.Cursor);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Select_Positions_In_Any_Order()
        {
            return StaRunner.Run(() =>
            {
                var run = new Run("some text");
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(run);

                Enable(textBlock).Select(PositionOf(run, 4), PositionOf(run, 0));

                Assert.Equal("some", TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("hello brave world", 8, "brave")]
        [InlineData("hello brave world", 6, "brave")]
        [InlineData("hello brave world", 11, "brave")]
        [InlineData("hello brave world", 0, "hello")]
        [InlineData("hello brave world", 17, "world")]
        [InlineData("foo.bar", 1, "foo")]
        [InlineData("snake_case2 x", 3, "snake_case2")]
        [InlineData("grüße", 2, "grüße")]
        [InlineData("a   b", 2, "")]
        [InlineData("...", 1, "")]
        public Task Should_Select_The_Word_At_A_Position(string text, int offset, string expected)
        {
            return StaRunner.Run(() =>
            {
                var run = new Run(text);
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(run);

                Enable(textBlock).SelectWord(PositionOf(run, offset));

                Assert.Equal(expected, TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Select_A_Word_Across_Inlines()
        {
            return StaRunner.Run(() =>
            {
                var bold = new Run("bold");
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(new Run("one half"));
                textBlock.Inlines.Add(new Bold(bold));
                textBlock.Inlines.Add(new Run("ly two"));

                Enable(textBlock).SelectWord(PositionOf(bold, 2));

                Assert.Equal("halfboldly", TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Stop_A_Word_At_A_LineBreak()
        {
            return StaRunner.Run(() =>
            {
                var run = new Run("first");
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(run);
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run("second"));

                Enable(textBlock).SelectWord(PositionOf(run, 2));

                Assert.Equal("first", TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Select_Across_Inlines()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(new Run("first"));
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Bold(new Run("second")));

                Enable(textBlock).SelectAll();

                Assert.Equal("first\r\nsecond", TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Clear_Selection_Without_Changing_The_Text()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                var behavior = Enable(textBlock);
                behavior.SelectAll();

                behavior.ClearSelection();

                Assert.Empty(TextBlockSelection.GetSelectedText(textBlock));
                Assert.Equal("some text", textBlock.Text);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Keep_Inlines_Untouched_When_Selecting()
        {
            return StaRunner.Run(() =>
            {
                var run = new Run("some text") { Foreground = Brushes.Red };
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(run);
                var behavior = Enable(textBlock);

                behavior.Select(PositionOf(run, 2), PositionOf(run, 4));
                behavior.ClearSelection();

                Assert.Single(textBlock.Inlines);
                Assert.Same(run, textBlock.Inlines.FirstInline);
                Assert.Equal(Brushes.Red, run.Foreground);
                Assert.Equal("some text", run.Text);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Clear_Selection_When_The_Text_Is_Replaced()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                var behavior = Enable(textBlock);
                Layout(textBlock);
                behavior.SelectAll();

                textBlock.Text = "other";
                Layout(textBlock);

                Assert.Empty(TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Clear_Selection_When_The_Text_Of_A_Run_Is_Replaced()
        {
            return StaRunner.Run(() =>
            {
                var run = new Run("some text");
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(run);
                var behavior = Enable(textBlock);
                Layout(textBlock);
                behavior.SelectAll();

                run.Text = "other";
                Layout(textBlock);

                Assert.Empty(TextBlockSelection.GetSelectedText(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Report_One_Area_Per_Line()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(new Run("first"));
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run("second line"));
                var behavior = Enable(textBlock);
                Layout(textBlock);

                behavior.SelectAll();

                var areas = behavior.GetSelectionAreas();

                Assert.Equal(2, areas.Count);
                Assert.True(areas[0].Width > 0);
                Assert.True(areas[1].Width > areas[0].Width);
                Assert.True(areas[1].Top >= areas[0].Bottom);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Report_One_Area_Per_Wrapped_Line()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "word word word word", TextWrapping = TextWrapping.Wrap, FontSize = 12 };
                var behavior = Enable(textBlock);
                Layout(textBlock, 50);

                behavior.SelectAll();

                Assert.True(behavior.GetSelectionAreas().Count > 1);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Report_No_Areas_Without_Selection()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock { Text = "some text" };
                var behavior = Enable(textBlock);
                Layout(textBlock);

                Assert.Empty(behavior.GetSelectionAreas());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Not_Be_A_Tab_Stop()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new TextBlock();
                Enable(textBlock);

                Assert.True(textBlock.Focusable);
                Assert.False(KeyboardNavigation.GetIsTabStop(textBlock));

                return Task.CompletedTask;
            });
        }
    }
}
