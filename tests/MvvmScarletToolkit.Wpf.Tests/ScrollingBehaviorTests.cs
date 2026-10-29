using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace MvvmScarletToolkit.Wpf.Tests
{
    internal static class ScrollingTestData
    {
        public static ListBox CreateListBox(int count)
        {
            var listBox = new ListBox { Height = 100 };
            for (var i = 0; i < count; i++)
            {
                listBox.Items.Add($"Item {i}");
            }

            return listBox;
        }

        public static ScrollViewer GetScrollViewer(FrameworkElement element)
        {
            return element.FindVisualChildDepthFirst<ScrollViewer>() ?? throw new InvalidOperationException("ScrollViewer not found");
        }
    }

    public sealed class AutoScrollBehaviorTests : TraceTestBase
    {
        [Fact]
        public Task Should_Scroll_To_New_Items()
        {
            return StaRunner.Run(async () =>
            {
                var listBox = ScrollingTestData.CreateListBox(50);
                Interaction.GetBehaviors(listBox).Add(new AutoScrollBehavior());

                var window = Ui.Show(listBox);
                await Ui.Settle();

                var scrollViewer = ScrollingTestData.GetScrollViewer(listBox);
                Assert.True(scrollViewer.VerticalOffset > 0);

                listBox.Items.Add("new");
                await Ui.Settle();

                Assert.Equal(scrollViewer.ScrollableHeight, scrollViewer.VerticalOffset);
                window.Close();
            });
        }

        [Fact]
        public Task Detaching_Should_Stop_Scrolling()
        {
            return StaRunner.Run(async () =>
            {
                var listBox = ScrollingTestData.CreateListBox(50);
                var behavior = new AutoScrollBehavior();
                Interaction.GetBehaviors(listBox).Add(behavior);

                var window = Ui.Show(listBox);
                await Ui.Settle();

                Interaction.GetBehaviors(listBox).Remove(behavior);

                var scrollViewer = ScrollingTestData.GetScrollViewer(listBox);
                scrollViewer.ScrollToTop();
                await Ui.Settle();

                listBox.Items.Add("new");
                await Ui.Settle();

                Assert.Equal(0, scrollViewer.VerticalOffset);
                window.Close();
            });
        }
    }

    public sealed class ScrollSelectionIntoViewBehaviorTests : TraceTestBase
    {
        [Fact]
        public Task Should_Scroll_Selected_Item_Into_View()
        {
            return StaRunner.Run(async () =>
            {
                var listBox = ScrollingTestData.CreateListBox(100);
                Interaction.GetBehaviors(listBox).Add(new ScrollSelectionIntoViewBehavior());

                var window = Ui.Show(listBox);
                await Ui.Settle();

                listBox.SelectedIndex = 99;
                await Ui.Settle();

                var scrollViewer = ScrollingTestData.GetScrollViewer(listBox);
                Assert.Equal(scrollViewer.ScrollableHeight, scrollViewer.VerticalOffset);
                window.Close();
            });
        }
    }

    public sealed class ScrollToEndCommandBehaviorTests : TraceTestBase
    {
        [Fact]
        public async Task Should_Be_Constructible_Without_SynchronizationContext()
        {
            var behavior = await Task.Run(() => new ScrollToEndCommandBehavior(), TestContext.Current.CancellationToken);

            Assert.NotNull(behavior);
        }

        [Fact]
        public Task Should_Not_Throw_Without_ScrollViewer()
        {
            // StaRunner fails the test, if the Loaded handler throws
            return StaRunner.Run(async () =>
            {
                var border = new Border();
                Interaction.GetBehaviors(border).Add(new ScrollToEndCommandBehavior());

                var window = Ui.Show(border);
                await Ui.Settle();
                window.Close();
            });
        }

        [Fact]
        public Task Should_Execute_Command_When_Scrolled_To_End()
        {
            return StaRunner.Run(async () =>
            {
                var executions = 0;
                var listBox = ScrollingTestData.CreateListBox(100);
                Interaction.GetBehaviors(listBox).Add(new ScrollToEndCommandBehavior
                {
                    Command = new RelayCommand(() => executions++),
                    Interval = TimeSpan.FromMilliseconds(10),
                });

                var window = Ui.Show(listBox);
                await Ui.Settle();

                ScrollingTestData.GetScrollViewer(listBox).ScrollToBottom();

                await Ui.WaitUntil(() => executions > 0);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Execute_Command_After_Being_Loaded_Again()
        {
            return StaRunner.Run(async () =>
            {
                var executions = 0;
                var listBox = ScrollingTestData.CreateListBox(100);
                Interaction.GetBehaviors(listBox).Add(new ScrollToEndCommandBehavior
                {
                    Command = new RelayCommand(() => executions++),
                    Interval = TimeSpan.FromMilliseconds(10),
                });

                var window = Ui.Show(listBox);
                await Ui.Settle();

                window.Content = null;
                await Ui.Settle();
                window.Content = listBox;
                await Ui.Settle();

                ScrollingTestData.GetScrollViewer(listBox).ScrollToBottom();

                await Ui.WaitUntil(() => executions > 0);
                window.Close();
            });
        }

        [Fact]
        public Task Detaching_Should_Stop_Executing_The_Command()
        {
            return StaRunner.Run(async () =>
            {
                var executions = 0;
                var listBox = ScrollingTestData.CreateListBox(100);
                var behavior = new ScrollToEndCommandBehavior
                {
                    Command = new RelayCommand(() => executions++),
                    Interval = TimeSpan.FromMilliseconds(10),
                };
                Interaction.GetBehaviors(listBox).Add(behavior);

                var window = Ui.Show(listBox);
                await Ui.Settle();

                Interaction.GetBehaviors(listBox).Remove(behavior);
                ScrollingTestData.GetScrollViewer(listBox).ScrollToBottom();
                await Task.Delay(100);
                await Ui.Settle();

                Assert.Equal(0, executions);
                window.Close();
            });
        }
    }

    public sealed class AutoRepositionPopupBehaviorTests : TraceTestBase
    {
        [Fact]
        public Task Detaching_Should_Not_Dispose_The_Windows_HwndSource()
        {
            return StaRunner.Run(async () =>
            {
                var target = new Button();
                var popup = new Popup { PlacementTarget = target };
                var panel = new StackPanel();
                panel.Children.Add(target);
                panel.Children.Add(popup);

                var behavior = new AutoRepositionPopupBehavior();
                Interaction.GetBehaviors(popup).Add(behavior);

                var window = Ui.Show(panel);
                await Ui.Settle();

                var handle = new WindowInteropHelper(window).Handle;
                var source = HwndSource.FromHwnd(handle);
                Assert.NotNull(source);

                Interaction.GetBehaviors(popup).Remove(behavior);

                Assert.Multiple(() =>
                {
                    Assert.False(source.IsDisposed);
                    Assert.Same(source, HwndSource.FromHwnd(handle));
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Tolerate_Missing_PlacementTarget()
        {
            return StaRunner.Run(async () =>
            {
                var popup = new Popup();
                var behavior = new AutoRepositionPopupBehavior();
                Interaction.GetBehaviors(popup).Add(behavior);

                var window = Ui.Show(new StackPanel { Children = { popup } });
                await Ui.Settle();

                Interaction.GetBehaviors(popup).Remove(behavior);
                window.Close();
            });
        }
    }
}
