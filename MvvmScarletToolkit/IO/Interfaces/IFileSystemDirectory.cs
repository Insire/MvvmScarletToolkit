namespace MvvmScarletToolkit
{
    public interface IFileSystemDirectory : IFileSystemInfo
    {
        RangeObservableCollection<IFileSystemInfo> Children { get; }
    }
}
