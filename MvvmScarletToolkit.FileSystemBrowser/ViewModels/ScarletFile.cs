using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public class ScarletFile : ScarletFileSystemBase, IFileSystemFile
    {
        public ScarletFile(FileInfo info, IDepth depth, IFileSystemDirectory parent)
            : base(info.Name, info.FullName, depth, parent)
        {
        }

        public override Task OnFilterChanged(string filter, CancellationToken token)
        {
            Filter = filter;
            return Task.CompletedTask;
        }

        public override Task LoadMetaData(CancellationToken token)
        {
            var info = new FileInfo(FullName);

            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;

            return Task.CompletedTask;
        }

        public override async Task Delete(CancellationToken token)
        {
            File.Delete(FullName);
            await Parent.RefreshCommand.ExecuteAsync(token).ConfigureAwait(false);
        }

        public override bool CanDelete()
        {
            return File.Exists(FullName);
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                {
                    await RefreshInternal(token).ConfigureAwait(false);
                    IsExpanded = true;
                }
            }
        }

        protected override Task UnloadInternalAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await OnFilterChanged(string.Empty, token).ConfigureAwait(false);
            }
        }
    }
}
