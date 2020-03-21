using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    public interface IFileSystemDirectory : IFileSystemInfo
    {
        IReadOnlyCollection<IFileSystemInfo> Children { get; }
    }
}
