using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    public interface IFileSystemFile : IFileSystemInfo, IFileSystemChild, IDisposable
    {
        string Extension { get; }
    }
}
