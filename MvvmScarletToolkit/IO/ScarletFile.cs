using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public class ScarletFile : ScarletFileSystemBase, IFileSystemInfo, IFileSystemFile
    {
        public ScarletFile(FileInfo info, IDepth depth) : base(info.Name, info.FullName, depth)
        {
            using (_busyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                {
                    Refresh();
                    IsExpanded = true;
                }
            }
        }

        public override void Refresh()
        {
            using (_busyStack.GetToken())
            {
                OnFilterChanged(string.Empty);
            }
        }

        public override void OnFilterChanged(string filter)
        {
            Filter = filter;
        }

        public override void LoadMetaData()
        {
            var info = new FileInfo(FullName);

            Exists = info.Exists;
            IsHidden = info.Attributes.HasFlag(FileAttributes.Hidden);
        }
    }
}
