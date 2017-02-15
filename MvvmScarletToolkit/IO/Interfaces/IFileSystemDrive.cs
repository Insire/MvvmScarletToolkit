namespace MvvmScarletToolkit
{
    public interface IFileSystemDrive : IFileSystemInfo
    {
        RangeObservableCollection<IFileSystemInfo> Children { get; }
    }
}
