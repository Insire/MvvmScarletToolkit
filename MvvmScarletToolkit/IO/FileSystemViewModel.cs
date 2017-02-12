using System;
using System.IO;
using System.Linq;

namespace MvvmScarletToolkit
{
    public class FileSystemViewModel : ObservableObject
    {
        private RangeObservableCollection<ScarletDrive> _drives;
        public RangeObservableCollection<ScarletDrive> Drives
        {
            get { return _drives; }
            private set { SetValue(ref _drives, value); }
        }

        public FileSystemViewModel()
        {
            Drives = new RangeObservableCollection<ScarletDrive>();

            var drives = Environment.GetLogicalDrives()
                                    .Select(p => new DriveInfo(p))
                                    .Select(p => new ScarletDrive(p, new FileSystemDepth(1)))
                                    .ToList();
            Drives.AddRange(drives);
        }
    }
}
