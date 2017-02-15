using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public class ScarletDirectory : ScarletFileSystemContainerBase, IFileSystemDirectory
    {
        public ScarletDirectory(DirectoryInfo info, IDepth depth) : base(info.Name, info.FullName, depth)
        {
            using (_busyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                    Refresh();
            }
        }

        public override void LoadMetaData()
        {
            var info = new DirectoryInfo(FullName);

            Exists = info.Exists;
            IsHidden = info.Attributes.HasFlag(FileAttributes.Hidden);
        }
    }
}
