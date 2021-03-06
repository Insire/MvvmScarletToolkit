namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo : IVirtualizationViewModel
    {
        string Name { get; }
        string FullName { get; }

        bool IsSelected { get; set; }

        bool IsBusy { get; }
        bool IsContainer { get; }
    }
}
