# Exporting a DataGrid

The toolkit ships no export formats and needs no extra dependencies for this. You provide an ``IDataGridExporter``, which asks the grid for its data through standard .NET types and turns it into whatever you need.

```cs
public interface IDataGridExporter
{
    Task Export(IDataGridDataSource source, CancellationToken token);
}
```

Start an export from XAML. ``CommandTarget`` is required when the button is outside of the grid:

```xml
<Button Command="{x:Static mvvm:DataGridCommands.Export}"
        CommandParameter="{Binding CsvExporter}"
        CommandTarget="{Binding ElementName=Grid}"
        Content="Export" />

<Button Command="{x:Static mvvm:DataGridCommands.CancelExport}"
        CommandTarget="{Binding ElementName=Grid}"
        Content="Cancel" />
```

``DataGridExport`` provides attached properties for the grid:

| Property | Description |
|---|---|
| ``DataGridExport.Progress`` | An ``IProgress<int>`` receiving the number of rows read so far. |
| ``DataGridExport.ExceptionHandler`` | An ``IScarletExceptionHandler`` receiving exceptions of an exporter. Without one, they surface as unhandled dispatcher exceptions. |
| ``DataGridExport.IsExporting`` | Whether an export is running. Readonly. |

The grid can also be read directly, without an exporter: ``dataGrid.ToDataTable()``, ``dataGrid.ToDataTable<TItem>()``, ``dataGrid.ToEnumerable(mapper)`` and ``dataGrid.GetItems<TItem>()``.

## Choosing how to read the data

| Method | Use it for |
|---|---|
| ``ToDataTable()`` | Quick access without knowing the row type. Every value is read through the bindings of the columns, which requires the UI thread. |
| ``ToDataTable<TItem>()`` | Typed columns and raw values, read from the properties of the row type on a background thread. Columns that don't map to a property fall back to their bindings. |
| ``ToEnumerable(mapper)`` | Mapping each row to a type of your own, by hand or with ``DataRowMapper.ByPropertyName<T>()``. |
| ``GetItems<TItem>()`` | Very large grids. Provides column metadata and the rows themselves, nothing is copied. |

``ToDataTable`` keeps the application responsive for large grids, but the bindings take far longer than ``ToDataTable<TItem>()`` and a ``DataTable`` is limited to about 16.7 million rows. For millions of rows, use ``GetItems<TItem>()`` and write them straight to a stream.

## What a snapshot contains

- The visible columns in display order. Unbound template columns are skipped, give them a ``ClipboardContentBinding`` or ``SortMemberPath`` to include them.
- The rows left after filtering, in the order shown on screen.
- Snapshots are flat. Grouped rows keep their order, nested groups are walked depth first and rows of collapsed groups are included. Use ``GroupBy`` over the rows, if you need sections or subtotals.
- A paged grid only provides its current page.

``DataColumn.ColumnName`` is the binding path, which doesn't change with a localized header. ``DataColumn.Caption`` is the header, use it for output. Missing values are ``DBNull.Value``.

## Threading

- ``Export`` runs on the thread pool, so a slow exporter can't block the UI. UI objects like the clipboard have to be accessed through a dispatcher.
- All methods of ``IDataGridDataSource`` can be called from any thread.
- Property getters of the rows have to be safe to call from a background thread.
- Rows are read after they were captured. A row edited during an export may show old or new values.

## Examples

### CSV

This example writes rows directly, which also works for very large grids. The WPF sample contains a [complete version](../samples/MvvmScarletToolkit.Wpf.Samples/Features/DataGrid/CsvExporter.cs) based on [CsvHelper](https://joshclose.github.io/CsvHelper/).

```cs
public sealed class PersonCsvExporter : IDataGridExporter
{
    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var data = await source.GetItems<PersonViewModel>(token);

        await using var writer = new StreamWriter("people.csv");
        await writer.WriteLineAsync("Id;Name");

        foreach (var item in data.Items)
        {
            token.ThrowIfCancellationRequested();
            await writer.WriteLineAsync($"{item.Id};{Escape(item.Name)}");
        }
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;

        return value.IndexOfAny([';', '"', '\r', '\n']) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}
```

### JSON

```cs
public sealed class JsonExporter : IDataGridExporter
{
    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var rows = await source.ToEnumerable(DataRowMapper.ByPropertyName<PersonDto>(), token);

        await using var stream = File.Create("people.json");
        await JsonSerializer.SerializeAsync(stream, rows, cancellationToken: token);
    }
}
```

The same with a mapper written by hand:

```cs
var rows = await source.ToEnumerable(row => new PersonDto
{
    Id = row.Field<int>("Id"),
    Name = row.Field<string>("Name"),
}, token);
```

### XML

```cs
public sealed class XmlExporter : IDataGridExporter
{
    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var table = await source.ToDataTable<PersonViewModel>(token);
        table.TableName = "Person";

        table.WriteXml("people.xml");
    }
}
```

### HTML

```cs
public sealed class HtmlExporter : IDataGridExporter
{
    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var table = await source.ToDataTable<PersonViewModel>(token);

        await using var writer = new StreamWriter("people.html");
        await writer.WriteLineAsync("<table><tr>");

        foreach (DataColumn column in table.Columns)
        {
            await writer.WriteLineAsync($"<th>{WebUtility.HtmlEncode(column.Caption)}</th>");
        }

        await writer.WriteLineAsync("</tr>");

        foreach (DataRow row in table.Rows)
        {
            var cells = row.ItemArray.Select(p => $"<td>{WebUtility.HtmlEncode(p?.ToString())}</td>");
            await writer.WriteLineAsync($"<tr>{string.Concat(cells)}</tr>");
        }

        await writer.WriteLineAsync("</table>");
    }
}
```

### Clipboard

The clipboard requires the UI thread, so this example uses the ``IScarletDispatcher`` of the toolkit.

```cs
public sealed class ClipboardExporter : IDataGridExporter
{
    private readonly IScarletDispatcher _dispatcher;

    public ClipboardExporter(IScarletDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var table = await source.ToDataTable<PersonViewModel>(token);

        var lines = new List<string> { string.Join('\t', table.Columns.Cast<DataColumn>().Select(p => p.Caption)) };
        lines.AddRange(table.Rows.Cast<DataRow>().Select(row => string.Join('\t', row.ItemArray)));

        var text = string.Join(Environment.NewLine, lines);

        // the clipboard may be locked by another application, the second argument retries a few times
        await _dispatcher.Invoke(() => Clipboard.SetDataObject(text, true), token);
    }
}
```

### Excel

[ClosedXML](https://github.com/ClosedXML/ClosedXML) creates a worksheet from a ``DataTable``. Add the package to your application:

```bash
dotnet add package ClosedXML
```

```cs
public sealed class ExcelExporter : IDataGridExporter
{
    public async Task Export(IDataGridDataSource source, CancellationToken token)
    {
        var table = await source.ToDataTable<PersonViewModel>(token);

        // worksheets use the column names, the captions are what the user saw
        foreach (DataColumn column in table.Columns)
        {
            column.ColumnName = column.Caption;
        }

        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add(table, "People");
        workbook.SaveAs("people.xlsx");
    }
}
```
