using MvvmScarletToolkit.Abstractions;
using System;
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

        protected ScarletFileSystemContainerBase(string name, string fullName, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
            : base(name, fullName, parent)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            using (BusyStack.GetToken())
            {
                _children = new ObservableCollection<IFileSystemInfo>();

                Children = new ReadOnlyObservableCollection<IFileSystemInfo>(_children);
                IsContainer = true;
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

                await Dispatcher.Invoke(() =>
                 {
                     using (DefaultCollectionView.DeferRefresh())
                     {
                         DefaultCollectionView.Filter = SearchFilter;
                     }
                 }).ConfigureAwait(false);
            }
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Clear().ConfigureAwait(false);

                await Dispatcher.Invoke(() =>
                {
                    if (NoFilesCollectionView is null)
                    {
                        NoFilesCollectionView = CollectionViewSource.GetDefaultView(Children);
                    }

                    if (DefaultCollectionView is null)
                    {
                        DefaultCollectionView = CollectionViewSource.GetDefaultView(Children);
                    }

                    using (NoFilesCollectionView.DeferRefresh())
                    {
                        NoFilesCollectionView.Filter = NoFilesFilter;
                    }

                    using (DefaultCollectionView.DeferRefresh())
                    {
                        DefaultCollectionView.Filter = SearchFilter;
                    }
                }).ConfigureAwait(false);

                await AddRange(FileSystemExtensions.GetChildren(this, Dispatcher)).ConfigureAwait(false);
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

        public async Task Clear()
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => _children.Clear());
            }

            OnPropertyChanged(nameof(Count));
        }

        protected override async Task UnloadInternalAsync()
        {
            using (BusyStack.GetToken())
            {
                await Clear().ConfigureAwait(false);
                IsLoaded = false;
            }
        }
    }
}
