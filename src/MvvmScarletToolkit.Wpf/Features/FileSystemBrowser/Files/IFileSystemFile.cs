using System;

namespace MvvmScarletToolkit
{
    public interface IFileSystemFile : IFileSystemInfo, IFileSystemChild, IDisposable
    {
        string Extension { get; }
    }
}
