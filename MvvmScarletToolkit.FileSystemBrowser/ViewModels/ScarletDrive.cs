using MvvmScarletToolkit.Abstractions;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    [DebuggerDisplay("Drive: {Name} IsContainer: {IsContainer}")]
    public class ScarletDrive : ScarletFileSystemContainerBase, IFileSystemDrive
    {
        private string _driveFormat;
        [Bindable(true, BindingDirection.OneWay)]
        public string DriveFormat
        {
            get { return _driveFormat; }
            private set { SetValue(ref _driveFormat, value); }
        }

        private DriveType _driveType;
        [Bindable(true, BindingDirection.OneWay)]
        public DriveType DriveType
        {
            get { return _driveType; }
            private set { SetValue(ref _driveType, value); }
        }

        private bool _isReady;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsReady
        {
            get { return _isReady; }
            private set { SetValue(ref _isReady, value); }
        }

        private long _availableFreeSpace;
        [Bindable(true, BindingDirection.OneWay)]
        public long AvailableFreeSpace
        {
            get { return _availableFreeSpace; }
            private set { SetValue(ref _availableFreeSpace, value); }
        }

        private long _totalFreeSpace;
        [Bindable(true, BindingDirection.OneWay)]
        public long TotalFreeSpace
        {
            get { return _totalFreeSpace; }
            private set { SetValue(ref _totalFreeSpace, value); }
        }

        private long _totalSize;
        [Bindable(true, BindingDirection.OneWay)]
        public long TotalSize
        {
            get { return _totalSize; }
            private set { SetValue(ref _totalSize, value); }
        }

        public ScarletDrive(DriveInfo info, IDepth depth, IScarletDispatcher dispatcher)
            : base(info.Name, info.Name, depth, null, dispatcher)
        {
        }

        public override async Task LoadMetaData(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var info = await Task.Run(() => new DriveInfo(FullName), token).ConfigureAwait(false);

                DriveFormat = info.DriveFormat;
                DriveType = info.DriveType;
                IsReady = info.IsReady;
                AvailableFreeSpace = info.AvailableFreeSpace;
                TotalFreeSpace = info.TotalFreeSpace;
                TotalSize = info.TotalSize;
            }
        }

        public override Task Delete(CancellationToken token)
        {
            return Task.CompletedTask; // i dont think i want to implement this
        }

        public override bool CanDelete()
        {
            return false;
        }
    }
}
