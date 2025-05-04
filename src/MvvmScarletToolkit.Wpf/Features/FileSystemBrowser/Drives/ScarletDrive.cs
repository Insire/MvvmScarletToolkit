using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("Drive: {Name} IsContainer: {IsContainer}")]
    public sealed partial class ScarletDrive : ObservableObject, IFileSystemDrive
    {
        private readonly ObservableCollectionExtended<IFileSystemChild> _containers;
        private readonly ObservableCollectionExtended<IFileSystemChild> _items;
        private readonly SourceCache<IFileSystemChild, string> _cache;
        private readonly ScarletDriveMapper _mapper;
        private readonly CompositeDisposable _compositeDisposable;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string? DriveFormat { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DriveType DriveType { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsReady { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long AvailableFreeSpace { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long TotalFreeSpace { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long TotalSize { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsLoaded { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsToggled { get; set; }

        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemChild> Items { get; }
        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemChild> Containers { get; }
        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => true;

        public ScarletDrive(
            IScheduler scheduler,
            ScarletDriveInfo info,
            IFileSystemViewModelFactory factory,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            ArgumentNullException.ThrowIfNull(info);

            _containers = new ObservableCollectionExtended<IFileSystemChild>();
            _items = new ObservableCollectionExtended<IFileSystemChild>();
            _cache = new SourceCache<IFileSystemChild, string>(vm => vm.Name);
            _mapper = new ScarletDriveMapper(this, info, factory, fileAttributes, folderAttributes);

            var itemsSubscription = _cache.Connect()
                 .ObserveOn(scheduler)
                 .Bind(_items)
                 .Subscribe();

            var containersSubscription = _cache.Connect()
                .Filter(p => p.IsContainer)
                .ObserveOn(scheduler)
                .Bind(_containers)
                .Subscribe();

            _compositeDisposable = new CompositeDisposable(itemsSubscription, containersSubscription);

            Containers = new ReadOnlyObservableCollection<IFileSystemChild>(_containers);
            Items = new ReadOnlyObservableCollection<IFileSystemChild>(_items);
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        async partial void OnIsToggledChanged(bool value)
        {
            if (value && IsLoaded == false)
            {
                await Refresh(CancellationToken.None);
            }
        }

        async partial void OnIsSelectedChanged(bool value)
        {
            if (value && IsLoaded == false)
            {
                await Refresh(CancellationToken.None);
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
