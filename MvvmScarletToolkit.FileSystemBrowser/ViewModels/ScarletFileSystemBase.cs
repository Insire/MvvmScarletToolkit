using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public abstract class ScarletFileSystemBase : ViewModelBase, IFileSystemInfo
    {
        public static bool NoFilesFilter(object obj)
        {
            return obj is IFileSystemFile
                ? false
                : SearchFilter(obj);
        }

        public static bool SearchFilter(object obj)
        {
            if (!(obj is IFileSystemInfo info))
            {
                return false;
            }

            if (info.IsHidden || info.IsBusy)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(info.Filter))
            {
                return true;
            }

            return info.Name.IndexOf(info.Filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        protected IScarletDispatcher Dispatcher { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand DeleteCommand { get; protected set; }

        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand ToggleExpandCommand { get; protected set; }

        private IFileSystemDirectory _parent;
        [Bindable(true, BindingDirection.OneWay)]
        public IFileSystemDirectory Parent
        {
            get { return _parent; }
            protected set { SetValue(ref _parent, value); }
        }

        private string _name;
        [Bindable(true, BindingDirection.OneWay)]
        public string Name
        {
            get { return _name; }
            protected set { SetValue(ref _name, value); }
        }

        private string _fullName;
        [Bindable(true, BindingDirection.OneWay)]
        public string FullName
        {
            get { return _fullName; }
            protected set { SetValue(ref _fullName, value); }
        }

        private string _filter;
        [Bindable(true, BindingDirection.TwoWay)]
        public string Filter
        {
            get { return _filter; }
            set { SetValue(ref _filter, value); }
        }

        private bool _exists;
        [Bindable(true, BindingDirection.OneWay)]
        public bool Exists
        {
            get { return _exists; }
            protected set { SetValue(ref _exists, value); }
        }

        private bool _isExpanded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsExpanded
        {
            get { return _isExpanded; }
            protected set { SetValue(ref _isExpanded, value); }
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
            protected set { SetValue(ref _isHidden, value); }
        }

        private bool _isContainer;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsContainer
        {
            get { return _isContainer; }
            protected set { SetValue(ref _isContainer, value); }
        }

        private bool _hasContainers;

        [Bindable(true, BindingDirection.OneWay)]
        public bool HasContainers
        {
            get { return _hasContainers; }
            protected set { SetValue(ref _hasContainers, value); }
        }

        private DateTime _creationTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime CreationTimeUtc
        {
            get { return _creationTimeUtc; }
            protected set { SetValue(ref _creationTimeUtc, value); }
        }

        private DateTime _lastAccessTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime LastAccessTimeUtc
        {
            get { return _lastAccessTimeUtc; }
            protected set { SetValue(ref _lastAccessTimeUtc, value); }
        }

        private DateTime _lastWriteTimeUtc;
        [Bindable(true, BindingDirection.OneWay)]
        public DateTime LastWriteTimeUtc
        {
            get { return _lastWriteTimeUtc; }
            protected set { SetValue(ref _lastWriteTimeUtc, value); }
        }

        private ScarletFileSystemBase()
        {
            DeleteCommand = AsyncCommand.Create(Delete, CanDelete).AsSequential();
            ToggleExpandCommand = AsyncCommand.Create(Toggle, CanToggle).AsSequential();

            Exists = true;
            IsHidden = false;
            IsContainer = false;
            HasContainers = false;
        }

        protected ScarletFileSystemBase(string name, string fullName, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} can't be empty.", nameof(name));
            }

            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException($"{nameof(fullName)} can't be empty.", nameof(fullName));
            }

            if (!(this is IFileSystemDrive) && parent is null)
            {
                throw new ArgumentException($"{nameof(parent)} can't be empty.", nameof(parent));
            }

            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            using (BusyStack.GetToken())
            {
                Name = name;
                FullName = fullName;
                Parent = parent;
            }
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            if (IsLoaded)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await RefreshInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
            }
        }

        public abstract Task LoadMetaData(CancellationToken token);

        public abstract Task OnFilterChanged(string filter, CancellationToken token);

        public abstract Task Delete(CancellationToken token);

        public abstract bool CanDelete();

        protected async Task Toggle(CancellationToken token)
        {
            if (IsBusy)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                if (!IsExpanded)
                {
                    await LoadInternal(token).ConfigureAwait(false);
                    await Dispatcher.Invoke(() => IsExpanded = true);
                }
                else
                {
                    await Dispatcher.Invoke(() => IsExpanded = !IsExpanded);
                }
            }
        }

        private bool CanToggle()
        {
            return !IsBusy;
        }
    }
}
