using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed record ScarletFileSelected(ScarletFile File);

    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public sealed partial class ScarletFile : ObservableRecipient, IFileSystemFile
    {
        private readonly ScarletFileMapper _mapper;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? Parent { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? CreationTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastAccessTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastWriteTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool Exists { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsHidden { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsLoaded { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsAccessProhibited { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => false;

        public ScarletFile(
            ScarletFileInfo info,
            IFileSystemParent parent,
            IFileSystemViewModelFactory factory)
            : base(factory.Messenger)
        {
            ArgumentNullException.ThrowIfNull(info);

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _mapper = new ScarletFileMapper(this, info, factory);
            IsActive = true;
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        partial void OnIsSelectedChanged(bool value)
        {
            if (value)
            {
                Messenger.Send(new ScarletFileSelected(this));
            }
        }

        public void Dispose()
        {
            IsActive = false;
        }
    }
}
