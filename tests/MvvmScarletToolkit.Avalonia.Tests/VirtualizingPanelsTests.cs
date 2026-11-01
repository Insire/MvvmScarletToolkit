using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using MvvmScarletToolkit.Avalonia.Tests.Util;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    public sealed class VirtualizingPanelsTests : TraceTestBase
    {
        private const double TileSize = 50;

        private readonly AvaloniaPlatformFixture _platform;

        public VirtualizingPanelsTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
        }

        private static (Window Window, ItemsControl Items, ScrollViewer Scroll) Show(Panel panel, IEnumerable<int> items, Func<int, Control>? build = null)
        {
            var itemsControl = new ItemsControl
            {
                ItemsPanel = new FuncTemplate<Panel?>(() => panel),
                ItemsSource = items,
                ItemTemplate = new FuncDataTemplate<int>((item, _) => build?.Invoke(item) ?? new Border { Width = TileSize, Height = TileSize }),
            };

            var scroll = new ScrollViewer
            {
                Content = itemsControl,
                HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            };

            var window = CreateWindow(scroll);
            window.Show();
            Settle(scroll);

            return (window, itemsControl, scroll);
        }

        private static Window CreateWindow(Control content)
        {
            return new Window { Width = 500, Height = 300, Content = content };
        }

        private static void Settle(Control control)
        {
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
                control.UpdateLayout();
            }
        }

        private static int Realized(ItemsControl items)
        {
            return items.GetRealizedContainers().Count();
        }

        private static Control Text(int item)
        {
            return new Border { Width = 40 + (item % 7 * 20), Height = 20 + (item % 3 * 10) };
        }

        [Fact]
        public Task TilePanel_Only_Realizes_The_Viewport()
        {
            return _platform.RunOnUIThread(() =>
            {
                var (window, items, _) = Show(new VirtualizingTilePanel { ChildSize = TileSize }, Enumerable.Range(0, 10_000));

                // 10 columns, 300px high, plus the cache
                Assert.InRange(Realized(items), 60, 200);
                Assert.Equal(1000 * TileSize, items.Bounds.Height);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task TilePanel_Realizes_Items_After_Scrolling()
        {
            return _platform.RunOnUIThread(() =>
            {
                var (window, items, scroll) = Show(new VirtualizingTilePanel { ChildSize = TileSize }, Enumerable.Range(0, 10_000));

                scroll.Offset = new Vector(0, 500 * TileSize);
                Settle(scroll);

                Assert.NotNull(items.ContainerFromIndex(5000));
                Assert.Null(items.ContainerFromIndex(0));
                Assert.InRange(Realized(items), 60, 200);
                Assert.Equal(new Rect(TileSize, 500 * TileSize, TileSize, TileSize), items.ContainerFromIndex(5001)!.Bounds);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task TilePanel_Reuses_Containers_While_Scrolling()
        {
            return _platform.RunOnUIThread(() =>
            {
                var panel = new VirtualizingTilePanel { ChildSize = TileSize };
                var (window, _, scroll) = Show(panel, Enumerable.Range(0, 10_000));

                // containers get recycled after the new ones are realized, so the first jump has to create some
                scroll.Offset = new Vector(0, 300 * TileSize);
                Settle(scroll);
                var children = panel.Children.Count;

                scroll.Offset = new Vector(0, 600 * TileSize);
                Settle(scroll);
                scroll.Offset = new Vector(0, 900 * TileSize);
                Settle(scroll);

                Assert.Equal(children, panel.Children.Count);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task TilePanel_Follows_Added_And_Removed_Items()
        {
            return _platform.RunOnUIThread(() =>
            {
                var source = new ObservableCollection<int>(Enumerable.Range(0, 100));
                var (window, items, _) = Show(new VirtualizingTilePanel { ChildSize = TileSize }, source);
                var second = items.ContainerFromIndex(1);

                source.Insert(0, -1);
                Settle(items);

                Assert.Same(second, items.ContainerFromIndex(2));
                Assert.Equal(new Rect(2 * TileSize, 0, TileSize, TileSize), second!.Bounds);

                source.RemoveAt(0);
                source.RemoveAt(0);
                Settle(items);

                Assert.Same(second, items.ContainerFromIndex(0));
                Assert.Equal(10 * TileSize, items.Bounds.Height);

                source.Clear();
                Settle(items);

                Assert.Equal(0, Realized(items));

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task WrapPanel_Wraps_Items_Of_Different_Sizes()
        {
            return _platform.RunOnUIThread(() =>
            {
                var sizes = new[] { 200, 200, 200, 100 };
                var (window, items, _) = Show(new VirtualizingWrapPanel(), Enumerable.Range(0, 4), item => new Border { Width = sizes[item], Height = 20 + (item * 10) });

                Assert.Equal(new Rect(0, 0, 200, 30), items.ContainerFromIndex(0)!.Bounds);
                Assert.Equal(new Rect(200, 0, 200, 30), items.ContainerFromIndex(1)!.Bounds);
                Assert.Equal(new Rect(0, 30, 200, 50), items.ContainerFromIndex(2)!.Bounds);
                Assert.Equal(new Rect(200, 30, 100, 50), items.ContainerFromIndex(3)!.Bounds);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task WrapPanel_Only_Realizes_The_Viewport()
        {
            return _platform.RunOnUIThread(() =>
            {
                var (window, items, scroll) = Show(new VirtualizingWrapPanel(), Enumerable.Range(0, 100_000), Text);
                Settle(scroll);

                Assert.InRange(Realized(items), 10, 500);

                scroll.Offset = new Vector(0, items.Bounds.Height / 2);
                Settle(scroll);

                Assert.Null(items.ContainerFromIndex(0));
                Assert.InRange(Realized(items), 10, 500);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task WrapPanel_Scrolls_Horizontally_With_Vertical_Orientation()
        {
            return _platform.RunOnUIThread(() =>
            {
                var panel = new VirtualizingWrapPanel { Orientation = Orientation.Vertical, ItemWidth = TileSize, ItemHeight = TileSize };
                var itemsControl = new ItemsControl
                {
                    ItemsPanel = new FuncTemplate<Panel?>(() => panel),
                    ItemsSource = Enumerable.Range(0, 1000),
                };

                var scroll = new ScrollViewer
                {
                    Content = itemsControl,
                    HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                };

                var window = CreateWindow(scroll);
                window.Show();
                Settle(scroll);

                // 6 items per column
                Assert.Equal(167 * TileSize, itemsControl.Bounds.Width);
                Assert.Equal(new Rect(TileSize, 0, TileSize, TileSize), itemsControl.ContainerFromIndex(6)!.Bounds);
                Assert.InRange(Realized(itemsControl), 60, 200);

                window.Close();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task ScrollIntoView_Realizes_Item_Far_Outside_Of_The_Viewport()
        {
            return _platform.RunOnUIThread(() =>
            {
                var (window, items, scroll) = Show(new VirtualizingTilePanel { ChildSize = TileSize }, Enumerable.Range(0, 10_000));

                items.ScrollIntoView(9_999);
                Settle(scroll);

                Assert.NotNull(items.ContainerFromIndex(9_999));
                Assert.InRange(scroll.Offset.Y, (1000 * TileSize) - 300 - 1, (1000 * TileSize) - 300 + 1);

                window.Close();
                return Task.CompletedTask;
            });
        }
    }
}
