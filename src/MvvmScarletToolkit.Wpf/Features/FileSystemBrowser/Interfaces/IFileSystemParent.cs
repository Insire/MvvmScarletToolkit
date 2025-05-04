namespace MvvmScarletToolkit
{
    public interface IFileSystemParent : IFileSystemInfo
    {
        bool IsToggled { get; set; }
    }
}
