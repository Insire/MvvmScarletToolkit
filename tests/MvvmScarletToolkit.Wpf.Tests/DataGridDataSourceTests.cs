using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DataGridDataSourceTests : TraceTestBase
    {
        public sealed class Child
        {
            public string? City { get; set; }
        }

        public class Row
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public decimal? Price { get; set; }
            public Child? Child { get; set; }
        }

        public sealed class RowDto
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? NotInTheGrid { get; set; }
        }

        public sealed class RowWrongDto
        {
            public string? Id { get; set; }
        }

        private static List<Row> CreateRows()
        {
            return
            [
                new Row { Id = 1, Name = "a", Price = 1.5m, Child = new Child { City = "x" } },
                new Row { Id = 2, Name = null, Price = null, Child = null },
            ];
        }

        private static DataGrid CreateGrid(IEnumerable<Row> rows)
        {
            var grid = new DataGrid { AutoGenerateColumns = false, ItemsSource = rows };

            grid.Columns.Add(new DataGridTextColumn { Header = "Identifier", Binding = new Binding(nameof(Row.Id)) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Label", Binding = new Binding(nameof(Row.Name)) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Price", Binding = new Binding(nameof(Row.Price)) { StringFormat = "N2" } });

            return grid;
        }

        [Fact]
        public Task ToDataTable_Should_Use_Visible_Columns_In_Display_Order()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());
                grid.Columns[0].Visibility = Visibility.Collapsed;
                grid.Columns[2].DisplayIndex = 0;

                var table = await grid.ToDataTable();

                Assert.Equal([nameof(Row.Price), nameof(Row.Name)], table.Columns.Cast<DataColumn>().Select(p => p.ColumnName));
                Assert.Equal(["Price", "Label"], table.Columns.Cast<DataColumn>().Select(p => p.Caption));
            });
        }

        [Fact]
        public Task ToDataTable_Should_Provide_Typed_Values_Through_Bindings()
        {
            return StaRunner.Run(async () =>
            {
                var table = await CreateGrid(CreateRows()).ToDataTable();

                Assert.Equal(typeof(int), table.Columns[nameof(Row.Id)]!.DataType);
                Assert.Equal(typeof(decimal), table.Columns[nameof(Row.Price)]!.DataType);
                Assert.Equal(1.5m, table.Rows[0][nameof(Row.Price)]);
                Assert.Equal(DBNull.Value, table.Rows[1][nameof(Row.Price)]);
            });
        }

        [Fact]
        public Task ToDataTable_Should_Respect_Filter_And_Sorting()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(Enumerable.Range(1, 5).Select(i => new Row { Id = i }).ToList());
                grid.Items.Filter = p => ((Row)p).Id != 3;
                grid.Items.SortDescriptions.Add(new SortDescription(nameof(Row.Id), ListSortDirection.Descending));

                var table = await grid.ToDataTable<Row>();

                Assert.Equal([5, 4, 2, 1], table.Rows.Cast<DataRow>().Select(p => p.Field<int>(nameof(Row.Id))));
            });
        }

        [Fact]
        public Task ToDataTable_Should_Flatten_Groups_And_Keep_Their_Order()
        {
            return StaRunner.Run(async () =>
            {
                var rows = new[] { "b", "a", "b", "a" }.Select((name, i) => new Row { Id = i, Name = name }).ToList();
                var grid = CreateGrid(rows);
                grid.Items.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Row.Name)));

                var table = await grid.ToDataTable<Row>();

                Assert.Equal([0, 2, 1, 3], table.Rows.Cast<DataRow>().Select(p => p.Field<int>(nameof(Row.Id))));
            });
        }

        [Fact]
        public Task ToDataTable_Should_Skip_The_New_Item_Placeholder()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(new ObservableCollection<Row>(CreateRows()));
                grid.CanUserAddRows = true;
                grid.IsReadOnly = false;

                Assert.Equal(3, grid.Items.Count);

                var table = await grid.ToDataTable();

                Assert.Equal(2, table.Rows.Count);
            });
        }

        [Fact]
        public Task ToDataTable_Should_Skip_Unbound_Template_Columns()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());
                grid.Columns.Add(new DataGridTemplateColumn { Header = "Action" });

                var table = await grid.ToDataTable();

                Assert.Equal(3, table.Columns.Count);
            });
        }

        [Fact]
        public Task ToDataTable_Should_Read_Template_Columns_By_Their_Sort_Path()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());
                grid.Columns.Clear();
                grid.Columns.Add(new DataGridTemplateColumn { Header = "Name", SortMemberPath = nameof(Row.Name) });

                var table = await grid.ToDataTable();

                Assert.Equal("a", table.Rows[0][0]);
            });
        }

        [Fact]
        public Task ToDataTable_Should_Create_Unique_Names_For_Repeated_Paths()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());
                grid.Columns.Add(new DataGridTextColumn { Header = "Again", Binding = new Binding(nameof(Row.Id)) });

                var table = await grid.ToDataTable();

                Assert.Equal(nameof(Row.Id) + "_2", table.Columns[3].ColumnName);
            });
        }

        [Fact]
        public Task Generic_ToDataTable_Should_Read_Raw_Values_And_Property_Types()
        {
            return StaRunner.Run(async () =>
            {
                var table = await CreateGrid(CreateRows()).ToDataTable<Row>();

                Assert.Equal(typeof(decimal), table.Columns[nameof(Row.Price)]!.DataType);
                Assert.Equal(typeof(string), table.Columns[nameof(Row.Name)]!.DataType);
                Assert.Equal(1.5m, table.Rows[0][nameof(Row.Price)]);
                Assert.Equal(DBNull.Value, table.Rows[1][nameof(Row.Name)]);
            });
        }

        [Fact]
        public Task Generic_ToDataTable_Should_Resolve_Nested_Paths()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());
                grid.Columns.Add(new DataGridTextColumn { Header = "City", Binding = new Binding("Child.City") });

                var table = await grid.ToDataTable<Row>();

                Assert.Equal("x", table.Rows[0]["Child.City"]);
                Assert.Equal(DBNull.Value, table.Rows[1]["Child.City"]);
            });
        }

        [Fact]
        public Task Generic_ToDataTable_Should_Reject_Rows_Of_Another_Type()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());

                await Assert.ThrowsAsync<InvalidOperationException>(() => grid.ToDataTable<string>(TestContext.Current.CancellationToken));
            });
        }

        [Fact]
        public Task Generic_ToDataTable_Should_Read_In_The_Background()
        {
            return StaRunner.Run(async () =>
            {
                var uiThread = Environment.CurrentManagedThreadId;
                var threads = new HashSet<int>();
                var rows = new[] { new TrackingRow(threads) };

                var grid = new DataGrid { AutoGenerateColumns = false, ItemsSource = rows };
                grid.Columns.Add(new DataGridTextColumn { Binding = new Binding(nameof(TrackingRow.Value)) });

                await grid.ToDataTable<TrackingRow>();

                Assert.DoesNotContain(uiThread, threads);
            });
        }

        public sealed class TrackingRow(HashSet<int> threads)
        {
            public int Value
            {
                get
                {
                    threads.Add(Environment.CurrentManagedThreadId);
                    return 1;
                }
            }
        }

        [Fact]
        public Task Slow_Path_Should_Not_Starve_The_Dispatcher()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(Enumerable.Range(0, 20_000).Select(i => new Row { Id = i }).ToList());
                var turns = 0;

                var snapshot = grid.ToDataTable();
                while (!snapshot.IsCompleted)
                {
                    // has to be below the priority of the slices, otherwise they never get to run
                    turns++;
                    await Dispatcher.Yield(DispatcherPriority.ContextIdle);
                }

                Assert.Equal(20_000, (await snapshot).Rows.Count);
                Assert.True(turns > 1, $"the dispatcher only got {turns} turns");
            });
        }

        [Fact]
        public Task ToDataTable_Should_Observe_Cancellation()
        {
            return StaRunner.Run(async () =>
            {
                using var cancellation = new CancellationTokenSource();
                cancellation.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateGrid(CreateRows()).ToDataTable(cancellation.Token));
            });
        }

        [Fact]
        public Task ToDataTable_Should_Be_Callable_From_A_Background_Thread()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid(CreateRows());

                var table = await Task.Run(() => grid.ToDataTable());

                Assert.Equal(2, table.Rows.Count);
            });
        }

        [Fact]
        public Task ToDataTable_Should_Report_Progress()
        {
            return StaRunner.Run(async () =>
            {
                var progress = new SynchronousProgress();
                var grid = CreateGrid(CreateRows());
                DataGridExport.SetProgress(grid, progress);

                await grid.ToDataTable();

                Assert.Equal(2, progress.Values.Max());
            });
        }

        private sealed class SynchronousProgress : IProgress<int>
        {
            public List<int> Values { get; } = [];

            public void Report(int value)
            {
                lock (Values)
                {
                    Values.Add(value);
                }
            }
        }

        [Fact]
        public Task GetItems_Should_Provide_The_Rows_And_Column_Metadata()
        {
            return StaRunner.Run(async () =>
            {
                var rows = CreateRows();
                var grid = CreateGrid(rows);
                grid.Columns[1].Visibility = Visibility.Collapsed;

                var result = await grid.GetItems<Row>();

                Assert.Equal(rows, result.Items);
                Assert.Equal(["Identifier", "Price"], result.Columns.Select(p => p.Caption));
                Assert.Equal([nameof(Row.Id), nameof(Row.Price)], result.Columns.Select(p => p.Name));
            });
        }

        [Fact]
        public Task GetItems_Should_Be_Isolated_From_Later_Changes()
        {
            return StaRunner.Run(async () =>
            {
                var rows = new ObservableCollection<Row>(CreateRows());
                var result = await CreateGrid(rows).GetItems<Row>();

                rows.Clear();

                Assert.Equal(2, result.Items.Count);
            });
        }

        [Fact]
        public Task ToEnumerable_Should_Use_The_Mapper()
        {
            return StaRunner.Run(async () =>
            {
                var result = await CreateGrid(CreateRows()).ToEnumerable(row => row.Field<int>(nameof(Row.Id)) * 10);

                Assert.Equal([10, 20], result);
            });
        }

        [Fact]
        public Task ByPropertyName_Should_Map_Matching_Properties()
        {
            return StaRunner.Run(async () =>
            {
                var result = (await CreateGrid(CreateRows()).ToEnumerable(DataRowMapper.ByPropertyName<RowDto>())).ToArray();

                Assert.Equal(1, result[0].Id);
                Assert.Equal("a", result[0].Name);
                Assert.Null(result[1].Name);
                Assert.Null(result[0].NotInTheGrid);
            });
        }

        [Fact]
        public Task ByPropertyName_Should_Name_The_Column_On_A_Mismatch()
        {
            return StaRunner.Run(async () =>
            {
                var result = await CreateGrid(CreateRows()).ToEnumerable(DataRowMapper.ByPropertyName<RowWrongDto>());

                var exception = Assert.Throws<InvalidOperationException>(() => result.ToArray());

                Assert.Contains(nameof(Row.Id), exception.Message);
            });
        }

        private sealed class TestExporter(Func<IDataGridDataSource, CancellationToken, Task> export) : IDataGridExporter
        {
            public Task Export(IDataGridDataSource source, CancellationToken token)
            {
                return export(source, token);
            }
        }

        private static async Task WaitForExport(DataGrid grid)
        {
            while (DataGridExport.GetIsExporting(grid))
            {
                await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            }
        }

        [Fact]
        public Task Export_Should_Run_The_Exporter_On_The_Thread_Pool()
        {
            return StaRunner.Run(async () =>
            {
                var uiThread = Environment.CurrentManagedThreadId;
                var exporterThread = 0;
                DataTable? table = null;

                var grid = CreateGrid(CreateRows());
                var exporter = new TestExporter(async (source, token) =>
                {
                    exporterThread = Environment.CurrentManagedThreadId;
                    table = await source.ToDataTable<Row>(token);
                });

                DataGridCommands.Export.Execute(exporter, grid);
                await WaitForExport(grid);

                Assert.NotEqual(uiThread, exporterThread);
                Assert.Equal(2, table!.Rows.Count);
            });
        }

        [Fact]
        public Task Export_Should_Run_Once_At_A_Time_And_Be_Cancellable()
        {
            return StaRunner.Run(async () =>
            {
                var calls = 0;
                var canceled = false;
                var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var grid = CreateGrid(CreateRows());
                var exporter = new TestExporter(async (_, token) =>
                {
                    Interlocked.Increment(ref calls);
                    started.SetResult();
                    try
                    {
                        await Task.Delay(Timeout.Infinite, token);
                    }
                    catch (OperationCanceledException)
                    {
                        canceled = true;
                        throw;
                    }
                });

                Assert.False(DataGridCommands.CancelExport.CanExecute(null, grid));
                DataGridCommands.Export.Execute(exporter, grid);
                await started.Task;

                Assert.False(DataGridCommands.Export.CanExecute(exporter, grid));
                Assert.True(DataGridCommands.CancelExport.CanExecute(null, grid));
                DataGridCommands.Export.Execute(exporter, grid);

                DataGridCommands.CancelExport.Execute(null, grid);
                await WaitForExport(grid);

                Assert.Equal(1, calls);
                Assert.True(canceled);
                Assert.True(DataGridCommands.Export.CanExecute(exporter, grid));
            });
        }

        [Fact]
        public Task Export_Should_Pass_Exceptions_To_The_Handler()
        {
            return StaRunner.Run(async () =>
            {
                var handler = Substitute.For<IScarletExceptionHandler>();
                var failure = new InvalidOperationException("failed");
                var grid = CreateGrid(CreateRows());
                DataGridExport.SetExceptionHandler(grid, handler);

                DataGridCommands.Export.Execute(new TestExporter((_, _) => Task.FromException(failure)), grid);
                await WaitForExport(grid);

                await handler.Received(1).Handle(failure);
                Assert.False(DataGridExport.GetIsExporting(grid));
            });
        }
    }
}
