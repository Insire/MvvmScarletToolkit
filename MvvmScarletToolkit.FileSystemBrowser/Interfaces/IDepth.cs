namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IDepth
    {
        int Current { get; set; }

        int Maximum { get; }

        bool IsMaxReached { get; }
    }
}
