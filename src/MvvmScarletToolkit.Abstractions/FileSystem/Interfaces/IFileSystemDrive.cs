using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    public interface IFileSystemDrive : IFileSystemInfo
    {
        IReadOnlyCollection<IFileSystemInfo> Children { get; }
    }
}
