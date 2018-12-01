namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IFileSystemDrive : IFileSystemInfo
    {
        RangeObservableCollection<IFileSystemInfo> Children { get; }
    }
}
