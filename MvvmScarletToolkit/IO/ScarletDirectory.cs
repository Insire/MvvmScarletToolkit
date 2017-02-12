using System;
using System.IO;
using System.Linq;

namespace MvvmScarletToolkit
{
    public class ScarletDirectory : ScarletFileSystemBase, IFileSystemDirectory
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

        private RangeObservableCollection<IFileSystemInfo> _children;
        public RangeObservableCollection<IFileSystemInfo> Children
        {
            get { return _children; }
            private set { SetValue(ref _children, value); }
        }

        private ScarletDirectory() : base()
        {
            Children = new RangeObservableCollection<IFileSystemInfo>();
        }

        public ScarletDirectory(DirectoryInfo info) : this()
        {
            using (_busyStack.GetToken())
            {
                Name = info.Name;
                FullName = info.FullName;
            }
        }

        public override void Load()
        {
            using (_busyStack.GetToken())
            {
                Clear();

                var info = new DirectoryInfo(FullName);

                Exists = info.Exists;
                LastAccessTimeUtc = info.LastAccessTime;
                LastWriteTimeUtc = info.LastWriteTimeUtc;
                CreationTimeUtc = info.CreationTimeUtc;

                Children.AddRange(FileSystemExtensions.GetChildren(this));

                Length = Children.Sum(p => p.Length);

                IsLoaded = true;
            }
        }

        private void Clear()
        {
            Children.Clear();

            Length = 0;
            Exists = false;
            LastAccessTimeUtc = DateTime.MinValue;
            LastWriteTimeUtc = DateTime.MinValue;
            CreationTimeUtc = DateTime.MinValue;

            IsLoaded = false;
        }
    }
}
