using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemViewModel : ObservableObject
    {
        protected readonly BusyStack _busyStack;

        public ICommand SelectCommand { get; }

        private RangeObservableCollection<IFileSystemInfo> _selectedItems;
        public RangeObservableCollection<IFileSystemInfo> SelectedItems
        {
            get { return _selectedItems; }
            private set { SetValue(ref _selectedItems, value); }
        }

        private RangeObservableCollection<ScarletDrive> _drives;
        public RangeObservableCollection<ScarletDrive> Drives
        {
            get { return _drives; }
            private set { SetValue(ref _drives, value); }
        }

        private ScarletFileSystemContainerBase _selectedItem;
        public ScarletFileSystemContainerBase SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value, OnChanged: OnSelectedItemChanged); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set { SetValue(ref _filter, value, OnChanged: () => SelectedItem.OnFilterChanged(Filter)); }
        }

        private bool _displayListView;
        public bool DisplayListView
        {
            get { return _displayListView; }
            set { SetValue(ref _displayListView, value); }
        }

        public FileSystemViewModel()
        {
            _busyStack = new BusyStack((hasItems) => IsBusy = hasItems);
            SelectCommand = new RelayCommand<IFileSystemInfo>(SetSelectedItem, CanSetSelectedItem);

            using (_busyStack.GetToken())
            {
                DisplayListView = false;

                Drives = new RangeObservableCollection<ScarletDrive>();
                SelectedItems = new RangeObservableCollection<IFileSystemInfo>();

                var drives = DriveInfo.GetDrives()
                                        .Where(p => p.IsReady && p.DriveType != DriveType.CDRom && p.DriveType != DriveType.Unknown)
                                        .Select(p => new ScarletDrive(p, new FileSystemDepth(0)))
                                        .ToList();
                Drives.AddRange(drives);
            }
        }

        private void OnSelectedItemChanged()
        {
            SelectedItem.Load();
            SelectedItem.LoadMetaData();
        }

        public void SetSelectedItem(IFileSystemInfo item)
        {
            if (!(item is ScarletFileSystemContainerBase value))
            {
                return;
            }

            SelectedItem = value;
            SelectedItem.ExpandPath();
            SelectedItem.Parent.IsSelected = true;
        }

        private bool CanSetSelectedItem(IFileSystemInfo item)
        {
            return item is ScarletFileSystemContainerBase value && value != SelectedItem;
        }
    }
}
