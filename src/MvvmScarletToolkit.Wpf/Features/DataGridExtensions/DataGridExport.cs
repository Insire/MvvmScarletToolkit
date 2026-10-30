using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Attached properties for the exports started through <see cref="DataGridCommands"/>
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class DataGridExport
    {
        private static readonly ConditionalWeakTable<DataGrid, CancellationTokenSource> _running = [];

        /// <summary>
        /// Receives the number of rows read so far
        /// </summary>
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached(
            "Progress",
            typeof(IProgress<int>),
            typeof(DataGridExport),
            new PropertyMetadata(null));

        public static void SetProgress(DependencyObject element, IProgress<int>? value)
        {
            element.SetValue(ProgressProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IProgress<int>? GetProgress(DependencyObject element)
        {
            return (IProgress<int>?)element.GetValue(ProgressProperty);
        }

        /// <summary>
        /// Receives exceptions thrown by an exporter. Without a handler they surface as unhandled dispatcher exceptions.
        /// </summary>
        public static readonly DependencyProperty ExceptionHandlerProperty = DependencyProperty.RegisterAttached(
            "ExceptionHandler",
            typeof(IScarletExceptionHandler),
            typeof(DataGridExport),
            new PropertyMetadata(null));

        public static void SetExceptionHandler(DependencyObject element, IScarletExceptionHandler? value)
        {
            element.SetValue(ExceptionHandlerProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IScarletExceptionHandler? GetExceptionHandler(DependencyObject element)
        {
            return (IScarletExceptionHandler?)element.GetValue(ExceptionHandlerProperty);
        }

        private static readonly DependencyPropertyKey _isExportingPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsExporting",
            typeof(bool),
            typeof(DataGridExport),
            new PropertyMetadata(false));

        /// <summary>
        /// Whether an export is running on the grid
        /// </summary>
        public static readonly DependencyProperty IsExportingProperty = _isExportingPropertyKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetIsExporting(DependencyObject element)
        {
            return (bool)element.GetValue(IsExportingProperty);
        }

        internal static void CanExport(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is IDataGridExporter && sender is DataGrid dataGrid && !GetIsExporting(dataGrid);
        }

        internal static void CanCancelExport(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender is DataGrid dataGrid && GetIsExporting(dataGrid);
        }

        internal static void OnCancelExport(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid && _running.TryGetValue(dataGrid, out var source))
            {
                source.Cancel();
            }
        }

        internal static async void OnExport(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not DataGrid dataGrid || e.Parameter is not IDataGridExporter exporter || GetIsExporting(dataGrid))
            {
                return;
            }

            using var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            var source = dataGrid.AsDataSource();

            _running.Add(dataGrid, cancellation);
            SetIsExporting(dataGrid, true);

            try
            {
                // an exporter continuing on the UI thread after awaiting its data would block it while writing
                await Task.Run(() => exporter.Export(source, token), token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception ex) when (GetExceptionHandler(dataGrid) is not null)
            {
                await GetExceptionHandler(dataGrid)!.Handle(ex);
            }
            finally
            {
                _running.Remove(dataGrid);
                SetIsExporting(dataGrid, false);
            }
        }

        private static void SetIsExporting(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(_isExportingPropertyKey, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
