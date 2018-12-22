using MvvmScarletToolkit.Abstractions;
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
    public class FileSystemViewModel : ViewModelListBase<IFileSystemInfo>
    {
        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand SelectCommand { get; }
        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand RefreshFilterCommand { get; }

        private RangeObservableCollection<IFileSystemInfo> _selectedItems;
        [Bindable(true, BindingDirection.OneWay)]
        public RangeObservableCollection<IFileSystemInfo> SelectedItems
        {
            get { return _selectedItems; }
            private set { SetValue(ref _selectedItems, value); }
        }

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

        public FileSystemViewModel(IScarletDispatcher dispatcher, FileSystemOptionsViewModel options)
            : base(dispatcher)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            SelectCommand = AsyncCommand.Create<IFileSystemInfo>(SetSelectedItem, CanSetSelectedItem);
            RefreshFilterCommand = AsyncCommand.Create(RefreshFilterInternal, CanRefreshFilter);
            SelectedItems = new RangeObservableCollection<IFileSystemInfo>();
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
            return !IsBusy && !(SelectedItem is null) && !(Filter is null);
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
                await SelectedItem.ExpandPath().ConfigureAwait(false);
                await SelectedItem.LoadCommand.ExecuteAsync(null).ConfigureAwait(false);
                await SelectedItem.LoadMetaData(CancellationToken.None).ConfigureAwait(false);
            }
        }

        private bool CanSetSelectedItem(IFileSystemInfo item)
        {
            return item is ScarletFileSystemContainerBase value && value != SelectedItem;
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await AddRange(DriveInfo.GetDrives()
                    .Where(p => p.IsReady && p.DriveType != DriveType.CDRom && p.DriveType != DriveType.Unknown)
                    .Select(p => new ScarletDrive(p, Dispatcher)))
                    .ConfigureAwait(false);

                IsLoaded = true;
            }
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                Clear();

                await LoadInternal(token).ConfigureAwait(false);
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
