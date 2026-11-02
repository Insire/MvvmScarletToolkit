using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class VirtualizingTilePanelTests : TraceTestBase
    {
        private const double ChildSize = 50;

        private static async Task<(Window Window, ItemsControl ListBox, VirtualizingTilePanel Panel, ObservableCollection<int> Items)> Show(int count)
        {
            var panelFactory = new FrameworkElementFactory(typeof(VirtualizingTilePanel));
            panelFactory.SetValue(VirtualizingTilePanel.ChildSizeProperty, ChildSize);

            var items = new ObservableCollection<int>(Enumerable.Range(0, count));
            var listBox = Ui.CreateItemsControl(panelFactory, items);

            var window = Ui.Show(listBox, 300, 300);
            await Ui.Settle();

            return (window, listBox, listBox.FindVisualChildBreadthFirst<VirtualizingTilePanel>()!, items);
        }

        private static int ChildrenPerRow(VirtualizingTilePanel panel)
        {
            return (int)Math.Floor(panel.ViewportWidth / ChildSize);
        }

        private static Point GetPosition(ItemsControl listBox, VirtualizingTilePanel panel, int index)
        {
            var container = (UIElement)listBox.ItemContainerGenerator.ContainerFromIndex(index);

            return container.TranslatePoint(new Point(), panel);
        }

        [Fact]
        public Task Should_Only_Realize_Visible_Items()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                var rows = (int)Math.Ceiling(panel.ViewportHeight / ChildSize);

                Assert.Equal(rows * ChildrenPerRow(panel), panel.Children.Count);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Compute_The_Extent_From_All_Items()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                var perRow = ChildrenPerRow(panel);
                var rows = Math.Ceiling(1000d / perRow);

                Assert.Multiple(() =>
                {
                    Assert.Equal(rows * ChildSize, panel.ExtentHeight);
                    Assert.Equal(perRow * ChildSize, panel.ExtentWidth);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Arrange_Items_In_Rows()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000);

                var perRow = ChildrenPerRow(panel);

                Assert.Multiple(() =>
                {
                    Assert.Equal(new Point(0, 0), GetPosition(listBox, panel, 0));
                    Assert.Equal(new Point(ChildSize, 0), GetPosition(listBox, panel, 1));
                    Assert.Equal(new Point(0, ChildSize), GetPosition(listBox, panel, perRow));
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Realize_Other_Items_After_Scrolling()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000);

                panel.SetVerticalOffset(ChildSize * 100);
                await Ui.Settle();

                var firstVisible = 100 * ChildrenPerRow(panel);

                Assert.Multiple(() =>
                {
                    Assert.Equal(ChildSize * 100, panel.VerticalOffset);
                    Assert.Null(listBox.ItemContainerGenerator.ContainerFromIndex(0));
                    Assert.NotNull(listBox.ItemContainerGenerator.ContainerFromIndex(firstVisible));
                    Assert.Equal(new Point(0, ChildSize * 100), GetPosition(listBox, panel, firstVisible));
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Limit_The_Offset_To_The_Extent()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                panel.SetVerticalOffset(double.MaxValue);
                var end = panel.VerticalOffset;

                panel.SetVerticalOffset(-1);

                Assert.Multiple(() =>
                {
                    Assert.Equal(panel.ExtentHeight - panel.ViewportHeight, end);
                    Assert.Equal(0, panel.VerticalOffset);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Not_Scroll_When_Everything_Fits()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1);

                panel.SetVerticalOffset(10);

                Assert.Equal(0, panel.VerticalOffset);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Scroll_By_Lines_Pages_And_The_MouseWheel()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);
                var page = panel.ViewportHeight;

                panel.LineDown();
                var afterLineDown = panel.VerticalOffset;
                panel.MouseWheelDown();
                var afterWheelDown = panel.VerticalOffset;
                panel.PageDown();
                var afterPageDown = panel.VerticalOffset;
                panel.PageUp();
                var afterPageUp = panel.VerticalOffset;
                panel.MouseWheelUp();
                var afterWheelUp = panel.VerticalOffset;
                panel.LineUp();

                Assert.Multiple(() =>
                {
                    Assert.Equal(10, afterLineDown);
                    Assert.Equal(20, afterWheelDown);
                    Assert.Equal(20 + page, afterPageDown);
                    Assert.Equal(20, afterPageUp);
                    Assert.Equal(10, afterWheelUp);
                    Assert.Equal(0, panel.VerticalOffset);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Remove_The_Container_Of_A_Removed_Item()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, items) = await Show(3);

                items.RemoveAt(0);
                await Ui.Settle();

                Assert.Multiple(() =>
                {
                    Assert.Equal(2, panel.Children.Count);
                    Assert.Equal(1, ((ContentPresenter)listBox.ItemContainerGenerator.ContainerFromIndex(0)).Content);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Realize_Items_Added_Later()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, items) = await Show(0);

                items.Add(1);
                await Ui.Settle();

                Assert.Single(panel.Children);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Remove_All_Containers_When_The_Items_Are_Cleared()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, items) = await Show(10);

                items.Clear();
                await Ui.Settle();

                Assert.Empty(panel.Children);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Not_Support_Horizontal_Scrolling()
        {
            return StaRunner.Run(() =>
            {
                var panel = new VirtualizingTilePanel();

                Assert.Multiple(() =>
                {
                    Assert.Throws<InvalidOperationException>(panel.LineLeft);
                    Assert.Throws<InvalidOperationException>(panel.LineRight);
                    Assert.Throws<InvalidOperationException>(panel.PageLeft);
                    Assert.Throws<InvalidOperationException>(panel.PageRight);
                    Assert.Throws<InvalidOperationException>(panel.MouseWheelLeft);
                    Assert.Throws<InvalidOperationException>(panel.MouseWheelRight);
                    Assert.Throws<InvalidOperationException>(() => panel.SetHorizontalOffset(1));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task MakeVisible_Should_Not_Scroll()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                var result = panel.MakeVisible(panel.Children[panel.Children.Count - 1], new Rect(0, 0, ChildSize, ChildSize));

                Assert.Multiple(() =>
                {
                    Assert.Equal(new Rect(), result);
                    Assert.Equal(0, panel.VerticalOffset);
                });
                window.Close();
            });
        }
    }
}
