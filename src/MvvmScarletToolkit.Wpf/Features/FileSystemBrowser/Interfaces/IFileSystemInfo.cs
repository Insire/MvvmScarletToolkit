namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo
    {
        string Name { get; }

        string FullName { get; }

        bool IsAccessProhibited { get; }

        bool IsSelected { get; set; }

        bool IsContainer { get; }

        bool IsLoaded { get; }

        bool Exists { get; }
    }
}
