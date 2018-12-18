using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemViewModel : ViewModelListBase<IFileSystemInfo>
    {
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand SelectCommand { get; }

        private RangeObservableCollection<IFileSystemInfo> _selectedItems;
        [Bindable(true, BindingDirection.OneWay)]
        public RangeObservableCollection<IFileSystemInfo> SelectedItems
        {
            get { return _selectedItems; }
            private set { SetValue(ref _selectedItems, value); }
        }

        private string _filter;
        [Bindable(true, BindingDirection.TwoWay)]
        public string Filter
        {
            get { return _filter; }
            set { SetValue(ref _filter, value, OnChanged: () => SelectedItem.OnFilterChanged(Filter, CancellationToken.None)); }
        }

        private bool _displayListView;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool DisplayListView
        {
            get { return _displayListView; }
            set { SetValue(ref _displayListView, value); }
        }

        public FileSystemViewModel(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
            SelectCommand = AsyncCommand.Create<IFileSystemInfo>(SetSelectedItem, CanSetSelectedItem);
            SelectedItems = new RangeObservableCollection<IFileSystemInfo>();
        }

        protected override async void OnSelectedItemChanged()
        {
            using (BusyStack.GetToken())
            {
                await SelectedItem.LoadCommand.ExecuteAsync(null).ConfigureAwait(false);
                await SelectedItem.LoadMetaData(CancellationToken.None).ConfigureAwait(false);
            }
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
                SelectedItem.Parent.IsSelected = true;
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
                    .Select(p => new ScarletDrive(p, new FileSystemDepth(0), Dispatcher)))
                    .ConfigureAwait(false);

                IsLoaded = true;
            }
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                Clear();

                await AddRange(DriveInfo.GetDrives()
                    .Where(p => p.IsReady && p.DriveType != DriveType.CDRom && p.DriveType != DriveType.Unknown)
                    .Select(p => new ScarletDrive(p, new FileSystemDepth(0), Dispatcher)))
                    .ConfigureAwait(false);
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
