using MvvmScarletToolkit.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public class ScarletDirectory : ScarletFileSystemContainerBase
    {
        public ScarletDirectory(DirectoryInfo info, IDepth depth, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
            : base(info.Name, info.FullName, depth, parent, dispatcher)
        {
        }

        public override async Task LoadMetaData(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var info = await Task.Run(() => new DirectoryInfo(FullName), token).ConfigureAwait(false);

                Exists = info.Exists;
                IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            }
        }

        public override async Task Delete(CancellationToken token)
        {
            Directory.Delete(FullName);
            await Parent.RefreshCommand.ExecuteAsync(token).ConfigureAwait(false);
        }

        public override bool CanDelete()
        {
            return Directory.Exists(FullName);
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                if (!Depth.IsMaxReached)
                {
                    await RefreshInternal(token).ConfigureAwait(false);
                }
            }
        }

        protected override Task UnloadInternalAsync()
        {
            using (BusyStack.GetToken())
            {
                Clear();
                IsLoaded = false;
                return Task.CompletedTask;
            }
        }
    }
}
