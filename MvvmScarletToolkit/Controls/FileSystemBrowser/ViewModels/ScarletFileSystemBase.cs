using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public abstract class ScarletFileSystemBase : BusinessViewModelBase, IFileSystemInfo
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

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand DeleteCommand { get; protected set; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand ToggleExpandCommand { get; protected set; }

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

        private ScarletFileSystemBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DeleteCommand = commandBuilder
                .Create(Delete, CanDelete)
                .WithSingleExecution(CommandManager)
                .Build();

            ToggleExpandCommand = commandBuilder
                .Create(Toggle, CanToggle)
                .WithSingleExecution(CommandManager)
                .Build();

            Exists = true;
            IsHidden = false;
            IsContainer = false;
            HasContainers = false;
        }

        protected ScarletFileSystemBase(string name, string fullName, IFileSystemDirectory parent, ICommandBuilder commandBuilder)
            : this(commandBuilder)
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

            using (BusyStack.GetToken())
            {
                Name = name;
                FullName = fullName;
                Parent = parent;
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
                    await Load(token).ConfigureAwait(false);
                    await Dispatcher.Invoke(() => IsExpanded = true).ConfigureAwait(false);
                }
                else
                {
                    await Dispatcher.Invoke(() => IsExpanded = !IsExpanded).ConfigureAwait(false);
                }
            }
        }

        private bool CanToggle()
        {
            return !IsBusy;
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            // hm, not sure what to add here,
            // maybe this is useful as an extension point for expensive operations later on
            return Task.CompletedTask;
        }
    }
}
