namespace MvvmScarletToolkit
{
    public interface IMimeTypeResolver
    {
        string? Get(IFileSystemFile fileInfo);
    }
}
