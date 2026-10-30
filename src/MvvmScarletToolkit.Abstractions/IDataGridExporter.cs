using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Turns the content of a data grid into something else, like a file. Runs on the thread pool, so UI objects have to be accessed through a dispatcher.
    /// </summary>
    public interface IDataGridExporter
    {
        Task Export(IDataGridDataSource source, CancellationToken token);
    }
}
