namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IFileSystemDirectory : IFileSystemInfo
    {
        RangeObservableCollection<IFileSystemInfo> Children { get; }
    }
}
