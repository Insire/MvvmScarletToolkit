using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed partial class FileSystemViewModel : ObservableObject, IDisposable
    {
        private readonly ObservableCollectionExtended<IFileSystemDrive> _items;
        private readonly SourceCache<IFileSystemDrive, string> _cache;
        private readonly FileSystemViewModelMapper _mapper;
        private readonly CompositeDisposable _compositeDisposable;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsLoaded { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? SelectedContainer { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemChild? SelectedChild { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemChild? SelectedDetail { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? SelectedTreeDetail { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public FileSystemOptionsViewModel Options { get; }
        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemDrive> Items { get; }

        public FileSystemViewModel(
            IScheduler scheduler,
            IFileSystemViewModelFactory factory,
            FileSystemOptionsViewModel options)
        {
            ArgumentNullException.ThrowIfNull(factory);
            ArgumentNullException.ThrowIfNull(options);

            _items = new ObservableCollectionExtended<IFileSystemDrive>();
            _cache = new SourceCache<IFileSystemDrive, string>(vm => vm.Name);
            _mapper = new FileSystemViewModelMapper(this, factory);

            var itemsSubscription = _cache.Connect()
                 .ObserveOn(scheduler)
                 .SortAndBind(_items, SortExpressionComparer<IFileSystemDrive>.Ascending(p => p.Name))
                 .Subscribe();

            _compositeDisposable = new CompositeDisposable(itemsSubscription);

            Options = options;
            Items = new ReadOnlyObservableCollection<IFileSystemDrive>(_items);
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
