using System.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Captures columns and item references on the UI thread, which is quick, and reads values afterwards.
    /// Bindings can only be evaluated on the UI thread, so that happens in slices to keep the application responsive.
    /// </summary>
    internal sealed class DataGridDataSource : IDataGridDataSource
    {
        private const int ProgressInterval = 4096;

        private static readonly TimeSpan _sliceDuration = TimeSpan.FromMilliseconds(8);

        private readonly DataGrid _dataGrid;
        private readonly Dispatcher _dispatcher;
        private IProgress<int>? _progress;

        public DataGridDataSource(DataGrid dataGrid)
        {
            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
            _dispatcher = dataGrid.Dispatcher;
        }

        public Task<DataTable> ToDataTable(CancellationToken token)
        {
            return CreateDataTable(null, token);
        }

        public Task<DataTable> ToDataTable<TItem>(CancellationToken token)
        {
            return CreateDataTable(typeof(TItem), token);
        }

        public async Task<IEnumerable<TResult>> ToEnumerable<TResult>(Func<DataRow, TResult> mapper, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            // the snapshot has to be complete here, only the mapping is deferred
            var table = await ToDataTable(token).ConfigureAwait(false);

            return table.Rows.Cast<DataRow>().Select(mapper);
        }

        public async Task<DataGridItems<TItem>> GetItems<TItem>(CancellationToken token)
        {
            var snapshot = await Capture(token).ConfigureAwait(false);

            var items = new TItem[snapshot.Items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = snapshot.Items[i] is TItem item
                    ? item
                    : throw CreateItemTypeException(typeof(TItem), snapshot.Items[i]);
            }

            return new DataGridItems<TItem>(snapshot.Columns.Select(p => p.Info).ToArray(), items);
        }

        private async Task<DataTable> CreateDataTable(Type? itemType, CancellationToken token)
        {
            var snapshot = await Capture(token).ConfigureAwait(false);

            if (itemType is not null)
            {
                ValidateItems(snapshot, itemType);
            }

            var readers = snapshot.Columns.Select(p => ColumnReader.Create(p, itemType)).ToArray();

            await ReadOnUiThread(snapshot, readers.Where(p => p.RequiresUiThread).ToArray(), token).ConfigureAwait(false);

            return await Task.Run(() => Fill(snapshot, readers, token), token).ConfigureAwait(false);
        }

        private Task<Snapshot> Capture(CancellationToken token)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                _progress = DataGridExport.GetProgress(_dataGrid);

                var columns = new List<SnapshotColumn>();
                var names = new HashSet<string>();
                var visible = _dataGrid.Columns
                    .Where(p => p.Visibility == System.Windows.Visibility.Visible)
                    .OrderBy(p => p.DisplayIndex);

                foreach (var column in visible)
                {
                    var path = column.GetPropertyPath();

                    // there is nothing to read from an unbound template column
                    if (path is null)
                    {
                        continue;
                    }

                    var name = path;
                    for (var i = 2; !names.Add(name); i++)
                    {
                        name = $"{path}_{i}";
                    }

                    columns.Add(new SnapshotColumn(column, path, column.ClipboardContentBinding is not null, new DataGridColumnInfo(name, column.GetCaption(columns.Count))));
                }

                var items = new List<object>(_dataGrid.Items.Count);
                foreach (var item in _dataGrid.Items)
                {
                    if (!ReferenceEquals(item, CollectionView.NewItemPlaceholder))
                    {
                        items.Add(item);
                    }
                }

                return new Snapshot(columns.ToArray(), items.ToArray());
            }, DispatcherPriority.Normal, token).Task;
        }

        private async Task ReadOnUiThread(Snapshot snapshot, ColumnReader[] readers, CancellationToken token)
        {
            if (readers.Length == 0)
            {
                return;
            }

            var index = 0;
            var items = snapshot.Items;

            foreach (var reader in readers)
            {
                reader.Values = new object?[items.Length];
            }

            while (index < items.Length)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    var stopwatch = Stopwatch.StartNew();

                    do
                    {
                        foreach (var reader in readers)
                        {
                            reader.Values![index] = reader.ReadOnUiThread(items[index]);
                        }

                        index++;
                    }
                    while (index < items.Length && stopwatch.Elapsed < _sliceDuration);
                }, DispatcherPriority.Background, token).Task.ConfigureAwait(false);

                _progress?.Report(index);
            }
        }

        private DataTable Fill(Snapshot snapshot, ColumnReader[] readers, CancellationToken token)
        {
            var table = new DataTable();

            for (var i = 0; i < readers.Length; i++)
            {
                table.Columns.Add(new DataColumn(snapshot.Columns[i].Info.Name, readers[i].GetColumnType())
                {
                    Caption = snapshot.Columns[i].Info.Caption,
                });
            }

            var items = snapshot.Items;

            table.BeginLoadData();
            for (var row = 0; row < items.Length; row++)
            {
                var values = new object?[readers.Length];
                for (var column = 0; column < readers.Length; column++)
                {
                    values[column] = readers[column].Read(items[row], row) ?? DBNull.Value;
                }

                table.Rows.Add(values);

                if (row % ProgressInterval == 0)
                {
                    token.ThrowIfCancellationRequested();
                    _progress?.Report(row);
                }
            }

            table.EndLoadData();
            _progress?.Report(items.Length);

            return table;
        }

        private static void ValidateItems(Snapshot snapshot, Type itemType)
        {
            foreach (var item in snapshot.Items)
            {
                if (!itemType.IsInstanceOfType(item))
                {
                    throw CreateItemTypeException(itemType, item);
                }
            }
        }

        private static InvalidOperationException CreateItemTypeException(Type expected, object actual)
        {
            return new InvalidOperationException($"The data grid contains a row of type {actual.GetType().FullName}, which is not a {expected.FullName}.");
        }

        private sealed record Snapshot(SnapshotColumn[] Columns, object[] Items);

        private sealed record SnapshotColumn(DataGridColumn Column, string Path, bool HasClipboardBinding, DataGridColumnInfo Info);

        private sealed class ColumnReader
        {
            private readonly SnapshotColumn _column;
            private readonly PropertyAccessor? _accessor;

            public bool RequiresUiThread => _accessor is null;

            public object?[]? Values { get; set; }

            private ColumnReader(SnapshotColumn column, PropertyAccessor? accessor)
            {
                _column = column;
                _accessor = accessor;
            }

            public static ColumnReader Create(SnapshotColumn column, Type? itemType)
            {
                // objects with thread affinity can't be read in the background
                var canRead = itemType is not null && !typeof(DispatcherObject).IsAssignableFrom(itemType);

                return new ColumnReader(column, canRead ? PropertyAccessor.Get(itemType!, column.Path) : null);
            }

            public object? ReadOnUiThread(object item)
            {
                return _column.HasClipboardBinding
                    ? _column.Column.OnCopyingCellClipboardContent(item)
                    : PropertyAccessor.Get(item.GetType(), _column.Path)?.GetValue(item);
            }

            public object? Read(object item, int row)
            {
                return _accessor is null ? Values![row] : _accessor.GetValue(item);
            }

            public Type GetColumnType()
            {
                if (_accessor is not null)
                {
                    return Nullable.GetUnderlyingType(_accessor.PropertyType) ?? _accessor.PropertyType;
                }

                Type? result = null;
                foreach (var value in Values ?? [])
                {
                    if (value is null)
                    {
                        continue;
                    }

                    var type = value.GetType();
                    if (result is not null && result != type)
                    {
                        return typeof(object);
                    }

                    result = type;
                }

                return result ?? typeof(object);
            }
        }
    }
}
