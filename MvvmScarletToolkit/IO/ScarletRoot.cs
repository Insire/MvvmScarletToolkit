using System.IO;
using System.Linq;

namespace MvvmScarletToolkit
{
    public class ScarletDrive : ScarletFileSystemBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        private long _length;
        /// <summary>
        /// the filled space
        /// </summary>
        public long Length
        {
            get { return _length; }
            private set { SetValue(ref _length, value); }
        }

        private long _space;
        public long FreeSpace
        {
            get { return _space; }
            private set { SetValue(ref _space, value); }
        }

        private long _totalLength;
        public long TotalSize
        {
            get { return _totalLength; }
            private set { SetValue(ref _totalLength, value); }
        }

        private string _driveFormat;
        public string DriveFormat
        {
            get { return _name; }
            private set { SetValue(ref _driveFormat, value); }
        }

        private string _volumeLabel;
        public string VolumeLabel
        {
            get { return _volumeLabel; }
            private set { SetValue(ref _volumeLabel, value); }
        }

        private DriveType _driveType;
        public DriveType DriveType
        {
            get { return _driveType; }
            private set { SetValue(ref _driveType, value); }
        }

        private RangeObservableCollection<IFileSystemInfo> _children;
        public RangeObservableCollection<IFileSystemInfo> Children
        {
            get { return _children; }
            private set { SetValue(ref _children, value); }
        }

        private ScarletDrive():base()
        {
            Children = new RangeObservableCollection<IFileSystemInfo>();
        }

        public ScarletDrive(DriveInfo info, IDepth depth) : this()
        {
            using (_busyStack.GetToken())
            {
                Depth = depth;
                Depth.Depth++;

                Name = info.Name;
                Length = info.TotalSize - info.TotalFreeSpace;
                FreeSpace = info.TotalFreeSpace;
                TotalSize = info.TotalSize;
                DriveFormat = info.DriveFormat;
                DriveType = info.DriveType;
                VolumeLabel = info.VolumeLabel;

                if (Depth.CanLoad)
                {
                    Load();
                    IsExpanded = true;
                }
            }
        }

        public override void Load()
        {
            using (_busyStack.GetToken())
            {
                Clear();
                var info = new DriveInfo(Name);

                Children.AddRange(info.RootDirectory.GetChildren(Depth));

                Length = Children.Sum(p => p.Length);
                IsLoaded = true;
            }
        }

        private void Clear()
        {
            Children.Clear();

            Length = 0;
            Exists = false;

            IsLoaded = false;
        }
    }
}
