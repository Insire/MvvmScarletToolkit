using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public class ScarletDirectory : ScarletFileSystemContainerBase
    {
        public ScarletDirectory(DirectoryInfo info, IFileSystemDirectory parent, IScarletCommandBuilder commandBuilder)
            : base(info.Name, info.FullName, parent, commandBuilder)
        {
        }

        public override async Task LoadMetaData(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var info = await Task.Run(() => new DirectoryInfo(FullName), token).ConfigureAwait(false);
                await Dispatcher.Invoke(() =>
                {
                    Exists = info.Exists;
                    IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
                    CreationTimeUtc = info.CreationTimeUtc;
                    LastAccessTimeUtc = info.LastAccessTimeUtc;
                    LastWriteTimeUtc = info.LastWriteTimeUtc;
                }).ConfigureAwait(false);
            }
        }

        public override async Task Delete(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Task.Run(() => Directory.Delete(FullName, true)).ConfigureAwait(false);
                Parent?.RefreshCommand?.Execute(null);
            }
        }

        public override bool CanDelete()
        {
            return !IsBusy && Directory.Exists(FullName);
        }
    }
}
