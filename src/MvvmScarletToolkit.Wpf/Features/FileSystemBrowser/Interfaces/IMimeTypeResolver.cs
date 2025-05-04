using System.IO;

namespace MvvmScarletToolkit
{
    public interface IMimeTypeResolver
    {
        string? Get(FileInfo fileInfo);
    }
}
