using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories
{
    public interface IFileSystemDirectory : IFileSystemParent, IFileSystemChild, IDisposable;
}
