namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IMimeTypeResolver
    {
        string Get(ScarletFile fileInfo);
    }
}
