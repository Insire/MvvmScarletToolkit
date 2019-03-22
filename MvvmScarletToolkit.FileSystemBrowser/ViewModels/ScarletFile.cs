using MvvmScarletToolkit.Commands;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public class ScarletFile : ScarletFileSystemBase, IFileSystemFile
    {
        public ScarletFile(FileInfo info, IFileSystemDirectory parent, CommandBuilder commandBuilder)
            : base(info.Name, info.FullName, parent, commandBuilder)
        {
        }

        public override Task OnFilterChanged(string filter, CancellationToken token)
        {
            Filter = filter;
            return Task.CompletedTask;
        }

        public override async Task LoadMetaData(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var info = await Task.Run(() => new FileInfo(FullName), token).ConfigureAwait(false);

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
                await Task.Run(() => File.Delete(FullName)).ConfigureAwait(false);
                await Parent.RefreshCommand.ExecuteAsync(token).ConfigureAwait(false);
            }
        }

        public override bool CanDelete()
        {
            return !IsBusy && File.Exists(FullName);
        }

        protected override async Task Refresh(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await OnFilterChanged(string.Empty, token).ConfigureAwait(false);
            }
        }
    }
}
