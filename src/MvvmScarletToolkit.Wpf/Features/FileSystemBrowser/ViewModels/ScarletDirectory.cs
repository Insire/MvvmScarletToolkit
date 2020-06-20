using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public sealed class ScarletDirectory : BusinessViewModelListBase<IFileSystemChild>, IFileSystemDirectory
    {
        private readonly IFileSystemViewModelFactory _factory;
        private readonly FileAttributes _fileAttributes;
        private readonly FileAttributes _folderAttributes;

        private IFileSystemParent? _parent;
        [Bindable(true, BindingDirection.OneWay)]
        public IFileSystemParent? Parent
        {
            get { return _parent; }
            private set { SetValue(ref _parent, value); }
        }

        private string _name;
        [Bindable(true, BindingDirection.OneWay)]
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        private string _fullName;
        [Bindable(true, BindingDirection.OneWay)]
        public string FullName
        {
            get { return _fullName; }
            private set { SetValue(ref _fullName, value); }
        }

        private bool _exists;
        [Bindable(true, BindingDirection.OneWay)]
        public bool Exists
        {
            get { return _exists; }
            private set { SetValue(ref _exists, value); }
        }

        private bool _isSelected;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        private bool _isHidden;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsHidden
        {
            get { return _isHidden; }
            private set { SetValue(ref _isHidden, value); }
        }

        private DateTime? _creationTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? CreationTimeUtc
        {
            get { return _creationTimeUtc; }
            private set { SetValue(ref _creationTimeUtc, value); }
        }

        private DateTime? _lastAccessTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? LastAccessTimeUtc
        {
            get { return _lastAccessTimeUtc; }
            private set { SetValue(ref _lastAccessTimeUtc, value); }
        }

        private DateTime? _lastWriteTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime? LastWriteTimeUtc
        {
            get { return _lastWriteTimeUtc; }
            private set { SetValue(ref _lastWriteTimeUtc, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsContainer { get; }

        public ScarletDirectory(DirectoryInfo info, FileAttributes fileAttributes, FileAttributes folderAttributes, IFileSystemParent parent, IScarletCommandBuilder commandBuilder, IFileSystemViewModelFactory factory)
            : base(commandBuilder)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _name = info.Name ?? throw new ArgumentException(nameof(info.Name));
            _fullName = info.FullName ?? throw new ArgumentException(info.FullName);
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _fileAttributes = fileAttributes;
            _folderAttributes = folderAttributes;
            IsContainer = true;

            Set(info);
        }

        private void Set(DirectoryInfo info)
        {
            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var info = await Task.Run(() => new DirectoryInfo(FullName), token);
            await Dispatcher.Invoke(() => Set(info));

            if (!Exists)
            {
                return;
            }

            var isEmpty = await _factory.IsEmpty(this);
            if (isEmpty)
            {
                await Clear(token);
                return;
            }

            var children = await _factory.GetChildren(this, _fileAttributes, _folderAttributes);

            if (!IsLoaded)
            {
                await AddRange(children);
            }
            else
            {
                await Dispatcher.Invoke(() => Items.UpdateItems(children, FileSystemViewModelFactoryExtensions.IFileSystemChildComparer, FileSystemViewModelFactoryExtensions.IFileSystemChildMapper));
            }
        }
    }
}
