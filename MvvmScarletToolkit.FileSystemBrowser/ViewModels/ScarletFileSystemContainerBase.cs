using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public abstract class ScarletFileSystemContainerBase : ScarletFileSystemBase, IFileSystemDirectory
    {
        private readonly ObservableCollection<IFileSystemInfo> _children;
        protected readonly IScarletDispatcher Dispatcher;

        private ICollectionView _noFilesCollectionView;
        [Bindable(true, BindingDirection.OneWay)]
        public ICollectionView NoFilesCollectionView
        {
            get { return _noFilesCollectionView; }
            protected set { SetValue(ref _noFilesCollectionView, value); }
        }

        private ICollectionView _defaultCollectionView;
        [Bindable(true, BindingDirection.OneWay)]
        public ICollectionView DefaultCollectionView
        {
            get { return _defaultCollectionView; }
            protected set { SetValue(ref _defaultCollectionView, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public IReadOnlyCollection<IFileSystemInfo> Children { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public int Count => _children.Count;

        public IFileSystemInfo this[int index]
        {
            get { return _children[index]; }
        }

        protected ScarletFileSystemContainerBase(string name, string fullName, IDepth depth, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
            : base(name, fullName, depth, parent)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            using (BusyStack.GetToken())
            {
                IsContainer = true;

                _children = new ObservableCollection<IFileSystemInfo>();

                Children = new ReadOnlyObservableCollection<IFileSystemInfo>(_children);

                NoFilesCollectionView = CollectionViewSource.GetDefaultView(Children); // meh, this keeps any child class from being created on an other thread than the "UI thread"
                DefaultCollectionView = CollectionViewSource.GetDefaultView(Children);

                using (NoFilesCollectionView.DeferRefresh())
                {
                    NoFilesCollectionView.Filter = NoFilesFilter;
                }

                using (DefaultCollectionView.DeferRefresh())
                {
                    DefaultCollectionView.Filter = SearchFilter;
                }
            }
        }

        public override async Task OnFilterChanged(string filter, CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                Filter = filter;
                await Children.ForEachAsync(async child =>
                {
                    await child.LoadMetaData(token).ConfigureAwait(false);
                    child.Filter = filter;
                }).ConfigureAwait(false);

                using (DefaultCollectionView.DeferRefresh()) // TODO invoke via dispatcher
                {
                    DefaultCollectionView.Filter = SearchFilter;
                }
            }
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                Clear();
                await AddRange(FileSystemExtensions.GetChildren(this, Depth, Dispatcher)).ConfigureAwait(false);
                HasContainers = Children.Any(p => p is ScarletFileSystemContainerBase);

                await OnFilterChanged(string.Empty, token).ConfigureAwait(false);
            }
        }

        public virtual async Task Add(IFileSystemInfo item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _children.Add(item)).ConfigureAwait(false);

                OnPropertyChanged(nameof(Count));
            }
        }

        public virtual async Task AddRange(IEnumerable<IFileSystemInfo> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                {
                    await Add(item).ConfigureAwait(false);
                }
            }
        }

        public virtual void Remove(IFileSystemInfo item)
        {
            using (BusyStack.GetToken())
            {
                _children.Remove(item);
            }

            OnPropertyChanged(nameof(Count));
        }

        public virtual void RemoveRange(IEnumerable<IFileSystemInfo> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using (BusyStack.GetToken())
            {
                foreach (var item in items)
                {
                    Remove(item);
                }
            }
        }

        private void RemoveRange(IList items)
        {
            RemoveRange(items?.Cast<IFileSystemInfo>());
        }

        protected virtual bool CanRemove(IFileSystemInfo item)
        {
            return CanClear()
                && !(item is null)
                && _children.Contains(item);
        }

        protected virtual bool CanRemoveRange(IEnumerable<IFileSystemInfo> items)
        {
            return CanClear()
                && items?.Any(p => _children.Contains(p)) == true;
        }

        protected virtual bool CanRemoveRange(IList items)
        {
            return CanRemoveRange(items?.Cast<IFileSystemInfo>());
        }

        protected bool CanClear()
        {
            return _children.Count > 0;
        }

        public void Clear()
        {
            using (BusyStack.GetToken())
            {
                _children.Clear();
            }

            OnPropertyChanged(nameof(Count));
        }
    }
}
