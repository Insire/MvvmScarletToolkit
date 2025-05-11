namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemParent : IFileSystemInfo
    {
        bool IsToggled { get; set; }
    }
}
