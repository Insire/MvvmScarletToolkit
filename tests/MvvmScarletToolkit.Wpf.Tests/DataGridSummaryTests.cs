using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DataGridSummaryTests : TraceTestBase
    {
        private sealed class Row(int id, decimal? price, string? name)
        {
            public int Id { get; } = id;
            public decimal? Price { get; } = price;
            public string? Name { get; } = name;
        }

        private static readonly object[] _rows = [new Row(1, 1.5m, "b"), new Row(2, null, "a"), new Row(3, 2.5m, null)];

        [Theory]
        [InlineData(nameof(Row.Price), DataGridSummaryFunction.Count, 3)]
        [InlineData(nameof(Row.Id), DataGridSummaryFunction.Minimum, 1)]
        [InlineData(nameof(Row.Id), DataGridSummaryFunction.Maximum, 3)]
        [InlineData(nameof(Row.Name), DataGridSummaryFunction.Minimum, "a")]
        public void Calculate_Should_Support_Any_Comparable(string path, DataGridSummaryFunction function, object expected)
        {
            Assert.Equal(expected, DataGridSummaryCalculator.Calculate(_rows, path, function));
        }

        [Fact]
        public void Sum_And_Average_Should_Ignore_Nulls()
        {
            Assert.Equal(4m, DataGridSummaryCalculator.Calculate(_rows, nameof(Row.Price), DataGridSummaryFunction.Sum));
            Assert.Equal(2m, DataGridSummaryCalculator.Calculate(_rows, nameof(Row.Price), DataGridSummaryFunction.Average));
        }

        [Fact]
        public void Numeric_Functions_Should_Have_No_Result_Without_Numbers()
        {
            Assert.Null(DataGridSummaryCalculator.Calculate(_rows, nameof(Row.Name), DataGridSummaryFunction.Sum));
            Assert.Null(DataGridSummaryCalculator.Calculate(_rows, nameof(Row.Name), DataGridSummaryFunction.Average));
            Assert.Null(DataGridSummaryCalculator.Calculate([], nameof(Row.Id), DataGridSummaryFunction.Maximum));
        }

        private static (DataGrid Grid, DataGridSummaryBar Bar, ObservableCollection<Row> Rows) Create()
        {
            var rows = new ObservableCollection<Row>(_rows.Cast<Row>());
            var grid = new DataGrid { AutoGenerateColumns = false, ItemsSource = rows };

            var id = new DataGridTextColumn { Binding = new Binding(nameof(Row.Id)) };
            var price = new DataGridTextColumn { Binding = new Binding(nameof(Row.Price)) };
            DataGridSummaryBar.SetFunction(price, DataGridSummaryFunction.Sum);
            DataGridSummaryBar.SetFormat(price, "0.00");

            grid.Columns.Add(id);
            grid.Columns.Add(price);

            return (grid, new DataGridSummaryBar { DataGrid = grid }, rows);
        }

        private static async Task Calculated()
        {
            // the calculation runs on the thread pool
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            await Task.Delay(50);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        }

        [Fact]
        public Task Bar_Should_Have_A_Formatted_Cell_Per_Visible_Column()
        {
            return StaRunner.Run(async () =>
            {
                var (_, bar, _) = Create();
                await Calculated();

                Assert.Equal(2, bar.Cells.Count);
                Assert.Null(bar.Cells[0].Text);
                Assert.Equal(4m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[1].Text);
            });
        }

        [Theory]
        [InlineData(DataGridSummaryFunction.Sum, "Sum of Price")]
        [InlineData(DataGridSummaryFunction.Minimum, "Minimum of Price")]
        [InlineData(DataGridSummaryFunction.Count, "Number of rows")]
        [InlineData(DataGridSummaryFunction.None, null)]
        public Task Cells_Should_Describe_Their_Value(DataGridSummaryFunction function, string? expected)
        {
            return StaRunner.Run(() =>
            {
                var grid = new DataGrid { AutoGenerateColumns = false };
                var price = new DataGridTextColumn { Header = "Price", Binding = new Binding(nameof(Row.Price)) };
                DataGridSummaryBar.SetFunction(price, function);
                grid.Columns.Add(price);

                var bar = new DataGridSummaryBar { CanChangeFunction = false, DataGrid = grid };

                Assert.Equal(expected, bar.Cells[0].Description);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Descriptions_Should_Say_How_To_Change_The_Function()
        {
            return StaRunner.Run(() =>
            {
                var (_, bar, _) = Create();

                Assert.Equal("Right-click to add a summary", bar.Cells[0].Description);
                Assert.Contains("Right-click to change", bar.Cells[1].Description);

                return Task.CompletedTask;
            });
        }

        public enum Kind
        {
            First,
        }

        public sealed class TypedRow
        {
            public byte Small { get; set; }
            public double? Ratio { get; set; }
            public DateTime Created { get; set; }
            public TimeSpan? Duration { get; set; }
            public string? Text { get; set; }
            public bool IsActive { get; set; }
            public Guid Id { get; set; }
            public Kind Kind { get; set; }
            public object? Anything { get; set; }
        }

        private const DataGridSummaryFunction Numbers = DataGridSummaryFunction.Sum | DataGridSummaryFunction.Average | DataGridSummaryFunction.Minimum | DataGridSummaryFunction.Maximum | DataGridSummaryFunction.Count;
        private const DataGridSummaryFunction Ordered = DataGridSummaryFunction.Minimum | DataGridSummaryFunction.Maximum | DataGridSummaryFunction.Count;

        [Theory]
        [InlineData(nameof(TypedRow.Small), Numbers)]
        [InlineData(nameof(TypedRow.Ratio), Numbers)]
        [InlineData(nameof(TypedRow.Created), Ordered)]
        [InlineData(nameof(TypedRow.Duration), Ordered)]
        [InlineData(nameof(TypedRow.Text), DataGridSummaryFunction.Count)]
        [InlineData(nameof(TypedRow.IsActive), DataGridSummaryFunction.Count)]
        [InlineData(nameof(TypedRow.Id), DataGridSummaryFunction.Count)]
        [InlineData(nameof(TypedRow.Kind), DataGridSummaryFunction.Count)]
        [InlineData(nameof(TypedRow.Anything), DataGridSummaryFunction.Count)]
        public void Functions_Should_Depend_On_The_Type(string property, DataGridSummaryFunction expected)
        {
            Assert.Equal(expected, DataGridSummaryCalculator.GetFunctions(typeof(TypedRow).GetProperty(property)!.PropertyType));
        }

        private static string[] GetHeaders(ContextMenu? menu)
        {
            return menu!.Items.Cast<MenuItem>().Select(p => (string)p.Header).ToArray();
        }

        [Fact]
        public Task Menu_Should_Offer_What_Fits_The_Type()
        {
            return StaRunner.Run(() =>
            {
                var (_, bar, _) = Create();

                Assert.Equal(["None", "Sum", "Average", "Minimum", "Maximum", "Count"], GetHeaders(bar.CreateMenu(bar.Cells[1])));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Menu_Should_Know_The_Type_Without_Rows()
        {
            return StaRunner.Run(() =>
            {
                var (_, bar, rows) = Create();
                rows.Clear();

                Assert.Contains("Sum", GetHeaders(bar.CreateMenu(bar.Cells[1])));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Menu_Should_Keep_A_Function_That_Does_Not_Fit_The_Type()
        {
            return StaRunner.Run(() =>
            {
                var (grid, bar, _) = Create();
                grid.Columns.Add(new DataGridTextColumn { Binding = new Binding(nameof(Row.Name)) });
                DataGridSummaryBar.SetFunction(grid.Columns[2], DataGridSummaryFunction.Minimum);

                Assert.Equal(["None", "Minimum", "Count"], GetHeaders(bar.CreateMenu(bar.Cells[2])));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Functions_Should_Override_The_Suggestions()
        {
            return StaRunner.Run(() =>
            {
                var (grid, bar, _) = Create();

                DataGridSummaryBar.SetFunctions(grid.Columns[0], DataGridSummaryFunction.Maximum | DataGridSummaryFunction.Count);

                Assert.Equal(["None", "Maximum", "Count"], GetHeaders(bar.CreateMenu(bar.Cells[0])));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Changing_The_Function_Should_Update_The_Cell()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, _) = Create();
                await Calculated();

                var menu = bar.CreateMenu(bar.Cells[1])!;
                Assert.True(((MenuItem)menu.Items[1]).IsChecked);

                ((MenuItem)menu.Items[4]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                await Calculated();

                Assert.Equal(DataGridSummaryFunction.Maximum, DataGridSummaryBar.GetFunction(grid.Columns[1]));
                Assert.Equal(2.5m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[1].Text);
                Assert.Contains("Maximum of", bar.Cells[1].Description);
            });
        }

        [Fact]
        public Task Changing_A_Function_Should_Only_Touch_Its_Own_Cell()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, rows) = Create();
                DataGridSummaryBar.SetFunction(grid.Columns[0], DataGridSummaryFunction.Maximum);
                await Calculated();

                var cells = bar.Cells.ToArray();
                var untouched = new List<string?>();
                cells[0].PropertyChanged += (_, e) => untouched.Add(e.PropertyName);

                DataGridSummaryBar.SetFunction(grid.Columns[1], DataGridSummaryFunction.Average);
                await Calculated();

                Assert.Equal(cells, bar.Cells);
                Assert.Empty(untouched);
                Assert.Equal(2m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), cells[1].Text);
            });
        }

        [Fact]
        public Task The_Last_Function_Should_Win()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, _) = Create();

                DataGridSummaryBar.SetFunction(grid.Columns[1], DataGridSummaryFunction.Maximum);
                DataGridSummaryBar.SetFunction(grid.Columns[1], DataGridSummaryFunction.Minimum);
                await Calculated();

                Assert.Equal(1.5m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[1].Text);

                DataGridSummaryBar.SetFunction(grid.Columns[1], DataGridSummaryFunction.Maximum);
                DataGridSummaryBar.SetFunction(grid.Columns[1], DataGridSummaryFunction.None);
                await Calculated();

                Assert.Null(bar.Cells[1].Text);
            });
        }

        [Fact]
        public Task Changing_Can_Be_Disabled_For_The_Bar_And_For_A_Column()
        {
            return StaRunner.Run(() =>
            {
                var (grid, bar, _) = Create();

                DataGridSummaryBar.SetFunctions(grid.Columns[0], DataGridSummaryFunction.None);
                Assert.Null(bar.CreateMenu(bar.Cells[0]));
                Assert.NotNull(bar.CreateMenu(bar.Cells[1]));

                bar.CanChangeFunction = false;

                Assert.Null(bar.CreateMenu(bar.Cells[1]));
                Assert.DoesNotContain("Right-click", bar.Cells[1].Description);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Bar_Should_Follow_Column_Changes()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, _) = Create();

                grid.Columns[0].Visibility = Visibility.Collapsed;
                await Calculated();

                Assert.Single(bar.Cells);
                Assert.NotNull(bar.Cells[0].Text);
            });
        }

        [Fact]
        public Task Bar_Should_Follow_Display_Order()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, _) = Create();

                grid.Columns[1].DisplayIndex = 0;
                await Calculated();

                Assert.NotNull(bar.Cells[0].Text);
                Assert.Null(bar.Cells[1].Text);
            });
        }

        [Fact]
        public Task Bar_Should_Update_When_Rows_Change()
        {
            return StaRunner.Run(async () =>
            {
                var (_, bar, rows) = Create();

                rows.Add(new Row(4, 6, null));
                await Calculated();

                Assert.Equal(10m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[1].Text);
            });
        }

        [Fact]
        public Task Bar_Should_Only_Use_Filtered_Rows()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, bar, _) = Create();

                grid.Items.Filter = p => ((Row)p).Id == 1;
                await Calculated();

                Assert.Equal(1.5m.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[1].Text);
            });
        }

        [Fact]
        public Task Bar_Should_Stop_Following_A_Disconnected_Grid()
        {
            return StaRunner.Run(() =>
            {
                var (grid, bar, _) = Create();

                bar.DataGrid = null;
                grid.Columns.Clear();

                Assert.Empty(bar.Cells);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task GroupingBehavior_Should_Group_By_A_Dropped_Column()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, _, _) = Create();
                var builder = Utils.GetTestCommandBuilder(new ScarletDispatcher(Dispatcher.CurrentDispatcher), messenger: new CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger());
                var grouping = GroupingViewModel.Create<NotifyingRow>(builder, () => (ListCollectionView)CollectionViewSource.GetDefaultView(grid.ItemsSource));
                var behavior = new DataGridGroupingBehavior { Grouping = grouping };
                Interaction.GetBehaviors(grid).Add(behavior);

                await behavior.Drop(nameof(NotifyingRow.Name));
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                Assert.True(grouping.IsGroupedBy(nameof(NotifyingRow.Name)));
            });
        }

        [Fact]
        public Task GroupingBehavior_Should_Ignore_Paths_That_Are_Not_Offered()
        {
            return StaRunner.Run(async () =>
            {
                var (grid, _, _) = Create();
                var builder = Utils.GetTestCommandBuilder(new ScarletDispatcher(Dispatcher.CurrentDispatcher), messenger: new CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger());
                var grouping = GroupingViewModel.Create<NotifyingRow>(builder, () => (ListCollectionView)CollectionViewSource.GetDefaultView(grid.ItemsSource));
                var behavior = new DataGridGroupingBehavior { Grouping = grouping };
                Interaction.GetBehaviors(grid).Add(behavior);

                await behavior.Drop("Child.City");

                Assert.False(grouping.IsGroupedBy("Child.City"));
            });
        }

        private sealed class NotifyingRow : INotifyPropertyChanged
        {
            public string? Name { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void Raise()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }
}
