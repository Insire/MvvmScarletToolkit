using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit
{
    public interface IFileSystemParent : IBusinessViewModelListBase<IFileSystemChild>, IFileSystemInfo
    {
    }
}
