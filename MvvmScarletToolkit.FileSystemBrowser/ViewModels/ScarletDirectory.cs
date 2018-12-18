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
        public ScarletDirectory(DirectoryInfo info, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
            : base(info.Name, info.FullName, parent, dispatcher)
        {
        }

        public override async Task LoadMetaData(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var info = await Task.Run(() => new DirectoryInfo(FullName), token).ConfigureAwait(false);

                Exists = info.Exists;
                IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
                CreationTimeUtc = info.CreationTimeUtc;
                LastAccessTimeUtc = info.LastAccessTimeUtc;
                LastWriteTimeUtc = info.LastWriteTimeUtc;
            }
        }

        public override async Task Delete(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Task.Run(() => Directory.Delete(FullName, true)).ConfigureAwait(false);
                await Parent.RefreshCommand.ExecuteAsync(token).ConfigureAwait(false);
            }
        }

        public override bool CanDelete()
        {
            return !IsBusy && Directory.Exists(FullName);
        }
    }
}
