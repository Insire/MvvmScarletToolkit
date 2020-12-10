using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public sealed class ScarletFile : BusinessViewModelBase, IFileSystemFile
    {
        private IFileSystemParent? _parent;
        [Bindable(true, BindingDirection.OneWay)]
        public IFileSystemParent? Parent
        {
            get { return _parent; }
            private set { SetProperty(ref _parent, value); }
        }

        private string _name;
        [Bindable(true, BindingDirection.OneWay)]
        public string Name
        {
            get { return _name; }
            private set { SetProperty(ref _name, value); }
        }

        private string _fullName;
        [Bindable(true, BindingDirection.OneWay)]
        public string FullName
        {
            get { return _fullName; }
            private set { SetProperty(ref _fullName, value); }
        }

        private bool _exists;
        [Bindable(true, BindingDirection.OneWay)]
        public bool Exists
        {
            get { return _exists; }
            private set { SetProperty(ref _exists, value); }
        }

        private bool _isSelected;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _isHidden;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsHidden
        {
            get { return _isHidden; }
            private set { SetProperty(ref _isHidden, value); }
        }

        private DateTime? _creationTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? CreationTimeUtc
        {
            get { return _creationTimeUtc; }
            private set { SetProperty(ref _creationTimeUtc, value); }
        }

        private DateTime? _lastAccessTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? LastAccessTimeUtc
        {
            get { return _lastAccessTimeUtc; }
            private set { SetProperty(ref _lastAccessTimeUtc, value); }
        }

        private DateTime? _lastWriteTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? LastWriteTimeUtc
        {
            get { return _lastWriteTimeUtc; }
            private set { SetProperty(ref _lastWriteTimeUtc, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsContainer { get; }

        public ScarletFile(FileInfo info, IFileSystemParent parent, IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _name = info.Name ?? throw new ArgumentException(nameof(info.Name));
            _fullName = info.FullName ?? throw new ArgumentException(info.FullName);
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            IsContainer = false;

            Set(info);
        }

        private void Set(FileInfo info)
        {
            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var info = await Task.Run(() => new FileInfo(FullName), token);
            await Dispatcher.Invoke(() => Set(info));
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
