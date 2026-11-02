using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class VirtualizingWrapPanelTests : TraceTestBase
    {
        private const double ChildSize = 50;

        private static async Task<(Window Window, ItemsControl ListBox, VirtualizingWrapPanel Panel, ObservableCollection<int> Items)> Show(int count, Orientation orientation = Orientation.Horizontal)
        {
            var panelFactory = new FrameworkElementFactory(typeof(VirtualizingWrapPanel));
            panelFactory.SetValue(VirtualizingWrapPanel.ItemWidthProperty, ChildSize);
            panelFactory.SetValue(VirtualizingWrapPanel.ItemHeightProperty, ChildSize);
            panelFactory.SetValue(VirtualizingWrapPanel.OrientationProperty, orientation);

            var containerStyle = new Style(typeof(ContentPresenter));
            containerStyle.Setters.Add(new Setter(FrameworkElement.WidthProperty, ChildSize));
            containerStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, ChildSize));

            var items = new ObservableCollection<int>(Enumerable.Range(0, count));
            var listBox = Ui.CreateItemsControl(panelFactory, items);
            listBox.ItemContainerStyle = containerStyle;

            var window = Ui.Show(listBox, 300, 300);
            await Ui.Settle();

            return (window, listBox, listBox.FindVisualChildBreadthFirst<VirtualizingWrapPanel>()!, items);
        }

        private static int ChildrenPerSection(double size)
        {
            return (int)Math.Floor(size / ChildSize);
        }

        private static Point GetPosition(ItemsControl listBox, VirtualizingWrapPanel panel, int index)
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

                Assert.InRange(panel.Children.Count, 1, 100);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Wrap_Into_A_New_Row()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000);

                var perRow = ChildrenPerSection(panel.ActualWidth);

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
        public Task Should_Wrap_Into_A_New_Column_When_Vertical()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000, Orientation.Vertical);

                var perColumn = ChildrenPerSection(panel.ActualHeight);

                Assert.Multiple(() =>
                {
                    Assert.Equal(new Point(0, ChildSize), GetPosition(listBox, panel, 1));
                    Assert.Equal(new Point(ChildSize, 0), GetPosition(listBox, panel, perColumn));
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Scroll_By_Rows()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000);

                panel.SetVerticalOffset(10);
                await Ui.Settle();

                var firstVisible = 10 * ChildrenPerSection(panel.ActualWidth);

                Assert.Multiple(() =>
                {
                    Assert.Equal(10, panel.GetFirstVisibleSection());
                    Assert.Equal(firstVisible, panel.GetFirstVisibleIndex());
                    Assert.Null(listBox.ItemContainerGenerator.ContainerFromIndex(0));
                    Assert.Equal(new Point(0, 0), GetPosition(listBox, panel, firstVisible));
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Scroll_By_Columns_When_Vertical()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000, Orientation.Vertical);

                panel.SetHorizontalOffset(10);
                await Ui.Settle();

                var firstVisible = 10 * ChildrenPerSection(panel.ActualHeight);

                Assert.Multiple(() =>
                {
                    Assert.Equal(10, panel.GetFirstVisibleSection());
                    Assert.Equal(firstVisible, panel.GetFirstVisibleIndex());
                    Assert.Equal(new Point(0, 0), GetPosition(listBox, panel, firstVisible));
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
        public Task Should_Scroll_By_Sections_With_Lines()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                panel.LineDown();
                var afterLineDown = panel.VerticalOffset;
                panel.LineUp();

                Assert.Multiple(() =>
                {
                    Assert.Equal(1, afterLineDown);
                    Assert.Equal(0, panel.VerticalOffset);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Scroll_Most_Of_The_Viewport_With_Pages_And_The_MouseWheel()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);
                var page = panel.ViewportHeight * 0.8;

                panel.PageDown();
                var afterPageDown = panel.VerticalOffset;
                panel.MouseWheelDown();
                var afterWheelDown = panel.VerticalOffset;
                panel.MouseWheelUp();
                var afterWheelUp = panel.VerticalOffset;
                panel.PageUp();

                Assert.Multiple(() =>
                {
                    Assert.Equal(page, afterPageDown, 5);
                    Assert.Equal(page * 2, afterWheelDown, 5);
                    Assert.Equal(page, afterWheelUp, 5);
                    Assert.Equal(0, panel.VerticalOffset, 5);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Scroll_Horizontally_With_Lines_And_Pages_When_Vertical()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000, Orientation.Vertical);
                var page = panel.ViewportWidth * 0.8;

                panel.LineRight();
                var afterLineRight = panel.HorizontalOffset;
                panel.LineLeft();
                var afterLineLeft = panel.HorizontalOffset;
                panel.PageRight();
                var afterPageRight = panel.HorizontalOffset;
                panel.MouseWheelRight();
                panel.MouseWheelLeft();
                panel.PageLeft();

                Assert.Multiple(() =>
                {
                    Assert.Equal(1, afterLineRight);
                    Assert.Equal(0, afterLineLeft);
                    Assert.Equal(page, afterPageRight, 5);
                    Assert.Equal(0, panel.HorizontalOffset, 5);
                });
                window.Close();
            });
        }

        [Fact]
        public Task MakeVisible_Should_Scroll_One_Row_For_A_Child_Below_The_Viewport()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                var last = panel.Children[panel.Children.Count - 1];
                var result = panel.MakeVisible(last, Rect.Empty);

                Assert.Multiple(() =>
                {
                    Assert.True(result.Bottom > panel.ActualHeight);
                    Assert.Equal(1, panel.VerticalOffset);
                });
                window.Close();
            });
        }

        [Fact]
        public Task MakeVisible_Should_Not_Scroll_For_A_Visible_Child()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, _) = await Show(1000);

                var result = panel.MakeVisible(panel.Children[0], Rect.Empty);

                Assert.Multiple(() =>
                {
                    Assert.Equal(new Rect(0, 0, ChildSize, ChildSize), result);
                    Assert.Equal(0, panel.VerticalOffset);
                });
                window.Close();
            });
        }

        [Fact]
        public Task Should_Rearrange_When_Resized()
        {
            return StaRunner.Run(async () =>
            {
                var (window, listBox, panel, _) = await Show(1000);

                window.Width = 450;
                await Ui.Settle();

                var perRow = ChildrenPerSection(panel.ActualWidth);

                Assert.Equal(new Point(0, ChildSize), GetPosition(listBox, panel, perRow));
                window.Close();
            });
        }

        [Fact]
        public Task Should_Reset_The_Offset_When_The_Items_Change()
        {
            return StaRunner.Run(async () =>
            {
                var (window, _, panel, items) = await Show(1000);
                panel.SetVerticalOffset(10);
                await Ui.Settle();

                items.Add(1000);
                await Ui.Settle();

                Assert.Equal(0, panel.VerticalOffset);
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
        public Task Should_Handle_Scrolling_Before_The_First_Layout()
        {
            return StaRunner.Run(() =>
            {
                var panel = new VirtualizingWrapPanel();

                panel.SetVerticalOffset(10);
                panel.SetHorizontalOffset(10);
                panel.Resizing(panel, EventArgs.Empty);
                panel.CleanUpItems(0, 1);

                Assert.Multiple(() =>
                {
                    Assert.Equal(0, panel.VerticalOffset);
                    Assert.Equal(0, panel.HorizontalOffset);
                    Assert.Equal(0, panel.GetFirstVisibleSection());
                    Assert.Equal(0, panel.GetFirstVisibleIndex());
                });

                return Task.CompletedTask;
            });
        }
    }
}
