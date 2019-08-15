using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IVirtualizedListViewModel
    {
        Task Extend(IEnumerable<object> items, CancellationToken token);

        Task Deflate(object item, CancellationToken token);
    }
}
