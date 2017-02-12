using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo : INotifyPropertyChanged
    {
        ICommand LoadCommand { get; }
        ICommand ClearCommand { get; }

        string Name { get; }
        string FullName { get; }
        long Length { get; }
        bool Exists { get; }
        bool IsSelected { get; }
        bool IsLoaded { get; }
        bool IsBusy { get; }

        DateTime LastWriteTimeUtc { get; }
        DateTime LastAccessTimeUtc { get; }
        DateTime CreationTimeUtc { get; }
    }
}
