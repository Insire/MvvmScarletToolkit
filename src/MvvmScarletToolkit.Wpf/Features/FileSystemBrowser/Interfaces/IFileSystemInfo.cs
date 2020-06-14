using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo : IBusinessViewModelBase
    {
        string Name { get; }
        string FullName { get; }

        bool IsSelected { get; set; }

        bool IsBusy { get; }
        bool IsContainer { get; }
    }
}
