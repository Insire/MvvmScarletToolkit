using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Commands available on every <see cref="DataGrid"/>, so that a viewmodel doesn't need to know about the grid
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // usage:
    // <Button Command="{x:Static mvvm:DataGridCommands.Export}"
    //         CommandParameter="{Binding CsvExporter}"
    //         CommandTarget="{Binding ElementName=Grid}" />
    public static class DataGridCommands
    {
        /// <summary>
        /// Runs the <see cref="IDataGridExporter"/> passed as command parameter on the thread pool. A grid runs one export at a time.
        /// </summary>
        public static RoutedCommand Export { get; } = new RoutedCommand(nameof(Export), typeof(DataGridCommands));

        /// <summary>
        /// Cancels the running export
        /// </summary>
        public static RoutedCommand CancelExport { get; } = new RoutedCommand(nameof(CancelExport), typeof(DataGridCommands));

        static DataGridCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(Export, DataGridExport.OnExport, DataGridExport.CanExport));
            CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(CancelExport, DataGridExport.OnCancelExport, DataGridExport.CanCancelExport));
        }
    }
}
