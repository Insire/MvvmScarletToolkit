using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DataGridColumnTests : TraceTestBase
    {
        private sealed record Row(int Id, string Name);

        private static DataGrid CreateGrid()
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = new[] { new Row(2, "b"), new Row(1, "a") },
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Identifier", Binding = new Binding(nameof(Row.Id)) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Label", Binding = new Binding(nameof(Row.Name)) });

            return grid;
        }

        [Fact]
        public Task Chooser_Should_Toggle_Column_Visibility()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridColumnChooserBehavior();
                Interaction.GetBehaviors(grid).Add(behavior);

                var item = (MenuItem)behavior.CreateMenu().Items[1];
                item.IsChecked = false;
                item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Chooser_Should_Protect_The_Last_Visible_Column()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridColumnChooserBehavior();
                Interaction.GetBehaviors(grid).Add(behavior);

                var menu = behavior.CreateMenu();
                var item = (MenuItem)menu.Items[1];
                item.IsChecked = false;
                item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                Assert.False(((MenuItem)menu.Items[0]).IsEnabled);
                Assert.True(item.IsEnabled);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Layout_Should_Survive_A_Round_Trip_Through_Another_Grid()
        {
            return StaRunner.Run(() =>
            {
                var source = CreateGrid();
                source.Columns[0].Visibility = Visibility.Collapsed;
                source.Columns[1].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                source.Columns[1].DisplayIndex = 0;
                source.Items.SortDescriptions.Add(new SortDescription(nameof(Row.Name), ListSortDirection.Descending));

                var layout = source.CaptureLayout();

                var target = CreateGrid();
                target.ApplyLayout(layout);

                Assert.Equal(Visibility.Collapsed, target.Columns[0].Visibility);
                Assert.Equal(2, target.Columns[1].Width.Value);
                Assert.True(target.Columns[1].Width.IsStar);
                Assert.Equal(0, target.Columns[1].DisplayIndex);
                Assert.Equal(ListSortDirection.Descending, target.Columns[1].SortDirection);
                Assert.Equal(nameof(Row.Name), Assert.Single(target.Items.SortDescriptions).PropertyName);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Layout_Keys_Should_Not_Depend_On_Headers()
        {
            return StaRunner.Run(() =>
            {
                var layout = CreateGrid().CaptureLayout();

                Assert.Equal([nameof(Row.Id), nameof(Row.Name)], layout.Columns.Select(p => p.Key));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task ApplyLayout_Should_Ignore_Unknown_Entries()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                grid.ApplyLayout(new DataGridLayout { Columns = [new DataGridColumnLayout { Key = "Unknown", IsVisible = false }] });

                Assert.All(grid.Columns, p => Assert.Equal(Visibility.Visible, p.Visibility));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task LayoutBehavior_Should_Publish_Changes()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridLayoutBehavior();
                Interaction.GetBehaviors(grid).Add(behavior);

                grid.Columns[0].Visibility = Visibility.Collapsed;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                Assert.False(behavior.Layout!.Columns[0].IsVisible);
            });
        }

        [Fact]
        public Task LayoutBehavior_Should_Publish_Summary_Functions()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridLayoutBehavior();
                Interaction.GetBehaviors(grid).Add(behavior);

                DataGridSummaryBar.SetFunction(grid.Columns[0], DataGridSummaryFunction.Count);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                Assert.Equal(DataGridSummaryFunction.Count, behavior.Layout!.Columns[0].SummaryFunction);
            });
        }

        [Fact]
        public Task Summary_Functions_Should_Survive_A_Round_Trip()
        {
            return StaRunner.Run(() =>
            {
                var source = CreateGrid();
                DataGridSummaryBar.SetFunction(source.Columns[0], DataGridSummaryFunction.Maximum);

                var target = CreateGrid();
                DataGridSummaryBar.SetFunction(target.Columns[1], DataGridSummaryFunction.Sum);
                target.ApplyLayout(source.CaptureLayout());

                Assert.Equal(DataGridSummaryFunction.Maximum, DataGridSummaryBar.GetFunction(target.Columns[0]));
                Assert.Equal(DataGridSummaryFunction.None, DataGridSummaryBar.GetFunction(target.Columns[1]));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Layout_Should_Survive_System_Text_Json()
        {
            return StaRunner.Run(() =>
            {
                var source = CreateGrid();
                DataGridSummaryBar.SetFunction(source.Columns[0], DataGridSummaryFunction.Maximum);
                source.Columns[1].Visibility = Visibility.Collapsed;

                var json = System.Text.Json.JsonSerializer.Serialize(source.CaptureLayout());
                var target = CreateGrid();
                target.ApplyLayout(System.Text.Json.JsonSerializer.Deserialize<DataGridLayout>(json)!);

                Assert.Equal(DataGridSummaryFunction.Maximum, DataGridSummaryBar.GetFunction(target.Columns[0]));
                Assert.Equal(Visibility.Collapsed, target.Columns[1].Visibility);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task A_Layout_Without_A_Summary_Function_Should_Keep_The_One_Of_The_Column()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                DataGridSummaryBar.SetFunction(grid.Columns[0], DataGridSummaryFunction.Sum);
                var key = grid.CaptureLayout().Columns[0].Key;

                grid.ApplyLayout(new DataGridLayout { Columns = [new DataGridColumnLayout { Key = key, Width = 1, WidthType = DataGridLengthUnitType.Auto }] });

                Assert.Equal(DataGridSummaryFunction.Sum, DataGridSummaryBar.GetFunction(grid.Columns[0]));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task LayoutBehavior_Should_Apply_An_Assigned_Layout()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridLayoutBehavior();
                Interaction.GetBehaviors(grid).Add(behavior);

                behavior.Layout = new DataGridLayout { Columns = [new DataGridColumnLayout { Key = nameof(Row.Name), IsVisible = false, Width = 1, WidthType = DataGridLengthUnitType.Auto, DisplayIndex = 1 }] };

                Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);

                return Task.CompletedTask;
            });
        }
    }
}
