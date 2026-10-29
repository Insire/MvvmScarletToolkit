using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ListViewLayoutManagerTests : TraceTestBase
    {
        [Fact]
        public Task Should_Resize_Proportional_Columns_After_Being_Loaded_Again()
        {
            return StaRunner.Run(async () =>
            {
                var first = new GridViewColumn { Header = "first" };
                var second = new GridViewColumn { Header = "second" };
                ProportionalColumn.SetWidth(first, 1);
                ProportionalColumn.SetWidth(second, 1);

                var gridView = new GridView();
                gridView.Columns.Add(first);
                gridView.Columns.Add(second);

                var listView = new ListView { View = gridView };
                ListViewLayoutManager.SetEnabled(listView, true);

                var window = Ui.Show(listView, width: 200);
                await Ui.WaitUntil(() => first.ActualWidth > 0);
                var initialWidth = first.ActualWidth;

                window.Content = null;
                await Ui.Settle();
                window.Content = listView;
                await Ui.Settle();

                window.Width = 400;

                await Ui.WaitUntil(() => first.ActualWidth > initialWidth + 50);
                Assert.Equal(first.ActualWidth, second.ActualWidth, 1);
                window.Close();
            });
        }
    }
}
