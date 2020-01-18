using System;

namespace MvvmScarletToolkit
{
    public interface IVirtualizingCollectionViewSource : IDisposable
    {
        object GetItemAt(int index);
    }
}
