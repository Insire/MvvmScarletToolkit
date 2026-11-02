namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IViewModelMapper
    {
        Task Refresh(CancellationToken token);
    }
}
