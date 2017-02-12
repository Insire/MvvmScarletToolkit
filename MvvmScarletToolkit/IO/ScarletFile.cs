using System;
using System.IO;

namespace MvvmScarletToolkit
{
    public class ScarletFile : ScarletFileSystemBase, IFileSystemInfo
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            private set { SetValue(ref _fullName, value); }
        }

        private long _length;
        public long Length
        {
            get { return _length; }
            private set { SetValue(ref _length, value); }
        }

        private DateTime _lastWriteTimeUtc;
        public DateTime LastWriteTimeUtc
        {
            get { return _lastWriteTimeUtc; }
            private set { SetValue(ref _lastWriteTimeUtc, value); }
        }

        private DateTime _lastAccessTimeUtc;
        public DateTime LastAccessTimeUtc
        {
            get { return _lastAccessTimeUtc; }
            private set { SetValue(ref _lastAccessTimeUtc, value); }
        }

        private DateTime _creationTimeUtc;
        public DateTime CreationTimeUtc
        {
            get { return _creationTimeUtc; }
            private set { SetValue(ref _creationTimeUtc, value); }
        }

        private ScarletFile():base()
        {
            ClearCommand = new RelayCommand(Clear);
        }

        public ScarletFile(FileInfo info, IDepth depth) : this()
        {
            using (_busyStack.GetToken())
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));

                Depth = depth;
                Depth.Depth++;

                Name = info.Name;
                FullName = info.FullName;

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

                var info = new FileInfo(FullName);

                Name = info.Name;
                FullName = info.FullName;
                Length = info.Length;
                Exists = info.Exists;
                LastAccessTimeUtc = info.LastAccessTime;
                LastWriteTimeUtc = info.LastWriteTimeUtc;
                CreationTimeUtc = info.CreationTimeUtc;

                IsLoaded = true;
            }
        }

        private void Clear()
        {
            Length = 0;
            Exists = false;
            LastAccessTimeUtc = DateTime.MinValue;
            LastWriteTimeUtc = DateTime.MinValue;
            CreationTimeUtc = DateTime.MinValue;

            IsLoaded = false;
        }
    }
}
