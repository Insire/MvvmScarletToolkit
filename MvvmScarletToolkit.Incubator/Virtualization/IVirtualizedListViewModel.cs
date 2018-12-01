using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    public interface IVirtualizedListViewModel
    {
        void ExtendItems(IEnumerable<object> items);
        void DeflateItem(object item);
    }
}
