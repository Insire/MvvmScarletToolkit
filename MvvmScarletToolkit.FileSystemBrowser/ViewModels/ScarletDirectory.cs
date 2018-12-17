using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public class ScarletDirectory : ScarletFileSystemContainerBase
    {
        public ScarletDirectory(DirectoryInfo info, IDepth depth, IFileSystemDirectory parent)
            : base(info.Name, info.FullName, depth, parent)
        {
            using (BusyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                {
                    Refresh();
                }
            }
        }

        public override void LoadMetaData()
        {
            var info = new DirectoryInfo(FullName);

            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
        }

        public override void Delete()
        {
            Directory.Delete(FullName);
            Refresh();
        }

        public override bool CanDelete()
        {
            return Directory.Exists(FullName);
        }
    }
}
