using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemViewModel : BusinessViewModelListBase<IFileSystemInfo>
    {
        [Bindable(true, BindingDirection.OneWay)]
        public ConcurrentCommandBase SelectCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ConcurrentCommandBase RefreshFilterCommand { get; }

        private FileSystemOptionsViewModel _options;
        [Bindable(true, BindingDirection.OneWay)]
        public FileSystemOptionsViewModel Options
        {
            get { return _options; }
            private set { SetValue(ref _options, value); }
        }

        private string _filter;
        [Bindable(true, BindingDirection.TwoWay)]
        public string Filter
        {
            get { return _filter; }
            set { SetValue(ref _filter, value); }
        }

        public FileSystemViewModel(ICommandBuilder commandBuilder, FileSystemOptionsViewModel options)
            : base(commandBuilder)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            RefreshFilterCommand = commandBuilder.Create(RefreshFilterInternal, CanRefreshFilter);
            SelectCommand = commandBuilder.Create<IFileSystemInfo>(SetSelectedItem, CanSetSelectedItem);
        }

        private async Task RefreshFilterInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await SelectedItem.OnFilterChanged(Filter, token).ConfigureAwait(false);
            }
        }

        private bool CanRefreshFilter()
        {
            return !(SelectedItem is null) && !(Filter is null);
        }

        public async Task SetSelectedItem(IFileSystemInfo item)
        {
            if (!(item is ScarletFileSystemContainerBase value))
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                SelectedItem = value;
                SelectedItem.ExpandPath();
                await SelectedItem.LoadMetaData(CancellationToken.None).ConfigureAwait(false);
            }
        }

        private bool CanSetSelectedItem(IFileSystemInfo item)
        {
            return item is ScarletFileSystemContainerBase value && value != SelectedItem;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            await AddRange(DriveInfo.GetDrives()
                .Where(p => p.IsReady && p.DriveType != DriveType.CDRom && p.DriveType != DriveType.Unknown)
                .Select(p => new ScarletDrive(p, CommandBuilder)))
                .ConfigureAwait(false);
        }
    }
}
