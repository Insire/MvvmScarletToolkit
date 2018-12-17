using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public abstract class ScarletFileSystemBase : ObservableObject, IFileSystemInfo
    {
        protected readonly BusyStack BusyStack;

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
        public ICommand LoadCommand { get; protected set; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand RefreshCommand { get; protected set; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand DeleteCommand { get; protected set; }

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

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        private bool _isExpanded;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetValue(ref _isExpanded, value, OnChanged: OnExpandedChanged); }
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

        private IDepth _depth;
        [Bindable(true, BindingDirection.OneWay)]
        public IDepth Depth
        {
            get { return _depth; }
            protected set { SetValue(ref _depth, value); }
        }

        private ScarletFileSystemBase()
        {
            BusyStack = new BusyStack((hasItems) => IsBusy = hasItems);

            LoadCommand = new RelayCommand(Load, CanLoad);
            RefreshCommand = new RelayCommand(Refresh, CanRefresh);
            DeleteCommand = new RelayCommand(Delete, CanDelete);

            Exists = true;
            IsHidden = false;
            IsContainer = false;
            HasContainers = false;
        }

        protected ScarletFileSystemBase(string name, string fullName, IDepth depth, IFileSystemDirectory parent)
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

            if (depth == null)
            {
                throw new ArgumentException($"{nameof(depth)} can't be empty.", nameof(depth));
            }

            if (!(this is IFileSystemDrive) && parent is null)
            {
                throw new ArgumentException($"{nameof(parent)} can't be empty.", nameof(parent));
            }

            using (BusyStack.GetToken())
            {
                Depth = depth;
                Depth.Current++;

                Name = name;
                FullName = fullName;
                Parent = parent;
            }
        }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            Refresh();
        }

        public abstract void Refresh();

        public abstract void LoadMetaData();

        public abstract void OnFilterChanged(string filter);

        public abstract void Delete();

        public abstract bool CanDelete();

        protected bool CanLoad()
        {
            return !IsBusy && !IsLoaded;
        }

        protected bool CanRefresh()
        {
            return !IsBusy;
        }

        private void OnExpandedChanged()
        {
            if (!IsBusy && !IsLoaded)
            {
                Load();
                IsLoaded = true;
            }
        }
    }
}
