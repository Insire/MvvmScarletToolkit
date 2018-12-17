using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public abstract class ScarletFileSystemContainerBase : ScarletFileSystemBase, IFileSystemDirectory
    {
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

        private RangeObservableCollection<IFileSystemInfo> _children;
        [Bindable(true, BindingDirection.OneWay)]
        public RangeObservableCollection<IFileSystemInfo> Children
        {
            get { return _children; }
            private set { SetValue(ref _children, value); }
        }

        protected ScarletFileSystemContainerBase(string name, string fullName, IDepth depth, IFileSystemDirectory parent)
            : base(name, fullName, depth, parent)
        {
            using (BusyStack.GetToken())
            {
                IsContainer = true;

                Children = new RangeObservableCollection<IFileSystemInfo>();

                NoFilesCollectionView = CollectionViewSource.GetDefaultView(Children);
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

        public override void OnFilterChanged(string filter)
        {
            using (BusyStack.GetToken())
            {
                Filter = filter;
                Children.ToList().ForEach(p => p.LoadMetaData());
                Children.ToList().ForEach(p => p.Filter = filter);

                using (DefaultCollectionView.DeferRefresh())
                {
                    DefaultCollectionView.Filter = SearchFilter;
                }
            }
        }

        public override void Refresh()
        {
            using (BusyStack.GetToken())
            {
                Children.Clear();
                Children.AddRange(FileSystemExtensions.GetChildren(this, Depth));
                HasContainers = Children.Any(p => p is ScarletFileSystemContainerBase);

                OnFilterChanged(string.Empty);
            }
        }
    }
}
