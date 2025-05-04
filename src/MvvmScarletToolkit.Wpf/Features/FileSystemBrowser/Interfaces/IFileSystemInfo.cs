namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo
    {
        string Name { get; }
        string FullName { get; }

        bool IsSelected { get; set; }

        bool IsContainer { get; }
    }
}
