using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using MvvmScarletToolkit.Observables;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    /// <summary>
    /// Writes the visible columns of a grid to a file of the users choosing, and reports its progress.
    /// Uses <see cref="IDataGridDataSource.GetItems{TItem}"/>, so nothing is copied and this also works for very large grids.
    /// </summary>
    public sealed partial class CsvExporter<TItem> : ViewModelBase, IDataGridExporter
    {
        private static readonly TimeSpan _progressInterval = TimeSpan.FromMilliseconds(100);

        private readonly ExportFileDialog _dialog;
        private readonly IReadOnlyDictionary<string, Func<TItem, object?>> _readers;

        [ObservableProperty]
        public partial bool IsWriting { get; private set; }

        [ObservableProperty]
        public partial int RowsWritten { get; private set; }

        [ObservableProperty]
        public partial int RowCount { get; private set; }

        /// <param name="commandBuilder"></param>
        /// <param name="dialog"></param>
        /// <param name="readers">how to read the value of a column from a row, by the name of the column</param>
        public CsvExporter(IScarletCommandBuilder commandBuilder, ExportFileDialog dialog, IReadOnlyDictionary<string, Func<TItem, object?>> readers)
            : base(commandBuilder)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        public async Task Export(IDataGridDataSource source, CancellationToken token)
        {
            var path = await _dialog.Show("export.csv", token);
            if (path is null)
            {
                return;
            }

            // an existing file is only replaced once the export has succeeded
            var temporaryPath = path + ".tmp";

            try
            {
                var data = await source.GetItems<TItem>(token);

                await SetProgress(true, 0, data.Items.Count);
                await Write(data, temporaryPath, token);

                File.Move(temporaryPath, path, true);

                await ShowToast("Export finished", $"{data.Items.Count} rows written to {path}", ToastType.Success, false);
            }
            catch (OperationCanceledException)
            {
                File.Delete(temporaryPath);
                await ShowToast("Export canceled", "No file has been changed.", ToastType.Information, false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                await ShowToast("Export failed", ex.Message, ToastType.Error, true);
            }
            finally
            {
                await SetProgress(false, 0, 0);
            }
        }

        private async Task Write(DataGridItems<TItem> data, string path, CancellationToken token)
        {
            var columns = data.Columns.Where(p => _readers.ContainsKey(p.Name)).ToArray();
            var readers = columns.Select(p => _readers[p.Name]).ToArray();
            var stopwatch = Stopwatch.StartNew();

            await using var csv = CsvWriter.ToFile(path, new CsvWriterOptions { Separator = ';', FormatProvider = CultureInfo.CurrentCulture });

            csv.WriteHeader(columns.Select(p => p.Caption).ToArray());

            for (var i = 0; i < data.Items.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var item = data.Items[i];
                foreach (var reader in readers)
                {
                    csv.WriteField(reader(item));
                }

                await csv.EndRecordAsync(token);

                // by time, a fixed amount of rows would either be too much for the dispatcher or show nothing
                if (stopwatch.Elapsed >= _progressInterval)
                {
                    stopwatch.Restart();
                    await SetProgress(true, i, data.Items.Count);
                }
            }
        }

        private Task ShowToast(string title, string body, ToastType type, bool isPersistent)
        {
            // the default service is created on first use and needs the UI thread for that
            return Dispatcher.Invoke(() => ToastService.Default.Show(title, body, type, isPersistent));
        }

        private Task SetProgress(bool isWriting, int rowsWritten, int rowCount)
        {
            return Dispatcher.Invoke(() =>
            {
                IsWriting = isWriting;
                RowsWritten = rowsWritten;
                RowCount = rowCount;
            });
        }
    }
}
