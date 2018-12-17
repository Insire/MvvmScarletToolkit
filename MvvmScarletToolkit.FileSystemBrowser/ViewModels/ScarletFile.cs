using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public class ScarletFile : ScarletFileSystemBase, IFileSystemFile
    {
        public ScarletFile(FileInfo info, IDepth depth, IFileSystemDirectory parent)
            : base(info.Name, info.FullName, depth, parent)
        {
            using (BusyStack.GetToken())
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
            using (BusyStack.GetToken())
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
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
        }

        public override void Delete()
        {
            File.Delete(FullName);
            Parent.Refresh();
        }

        public override bool CanDelete()
        {
            return File.Exists(FullName);
        }
    }
}
