using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Drive: {Name} IsContainer: {IsContainer}")]
    public class ScarletDrive : ScarletFileSystemContainerBase, IFileSystemDrive
    {
        public ScarletDrive(DriveInfo info, IDepth depth) : base(info.Name, info.Name, depth)
        {
            using (_busyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                    Refresh();
            }
        }

        public override void LoadMetaData()
        {
        }
    }
}
