using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    public sealed record ScarletFileSelected(ScarletFile File);

    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public sealed partial class ScarletFile : ObservableRecipient, IFileSystemFile
    {
        private readonly ScarletFileMapper _mapper;
        private readonly ObservableCollectionExtended<PropertyViewModel> _properties;
        private readonly SourceCache<PropertyViewModel, string> _propertiesCache;
        private readonly CompositeDisposable _compositeDisposable;
        private readonly IObservableBusyStack _busyStack;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Extension { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string MimeType { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? Parent { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? CreationTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastAccessTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastWriteTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool Exists { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsHidden { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsLoaded { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsAccessProhibited { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsBusy { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsImage { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<PropertyViewModel> Properties { get; }

        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => false;
        [Bindable(true, BindingDirection.OneWay)] public FileSystemType FileSystemType { get; } = FileSystemType.File;

        public ScarletFile(
            IScheduler scheduler,
            ScarletFileInfo info,
            IFileSystemParent parent,
            IFileSystemViewModelFactory factory)
            : base(factory.Messenger)
        {
            ArgumentNullException.ThrowIfNull(info);

            _busyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);
            _properties = new ObservableCollectionExtended<PropertyViewModel>();
            _propertiesCache = new SourceCache<PropertyViewModel, string>(vm => vm.Key);
            _mapper = new ScarletFileMapper(this, info, factory);

            var propertiesSubscription = _propertiesCache.Connect()
                .ObserveOn(scheduler)
                .Bind(_properties)
                .Subscribe();

            _compositeDisposable = new CompositeDisposable(propertiesSubscription);

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Properties = new ReadOnlyObservableCollection<PropertyViewModel>(_properties);
            IsActive = true;
        }

        [RelayCommand]
        private void Select()
        {
            IsSelected = true;
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        partial void OnIsSelectedChanged(bool value)
        {
            PropertyViewModel.AddUpdateOrUpdateCache(_propertiesCache, 0, nameof(IsSelected), IsSelected ? bool.TrueString : bool.FalseString);

            if (value)
            {
                Messenger.Send(new ScarletFileSelected(this));
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            IsActive = false;
        }
    }
}
