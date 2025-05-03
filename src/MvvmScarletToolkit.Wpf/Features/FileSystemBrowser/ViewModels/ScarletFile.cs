using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("File: {Name} IsContainer: {IsContainer}")]
    public sealed partial class ScarletFile : BusinessViewModelBase, IFileSystemFile
    {
        private readonly IViewModelMapper _mapper;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? Parent { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool Exists { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsHidden { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? CreationTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastAccessTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastWriteTimeUtc { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => true;

        public ScarletFile(ScarletFileInfo info, IFileSystemParent parent, IScarletCommandBuilder commandBuilder, IFileSystemViewModelFactory factory)
            : base(commandBuilder)
        {
            ArgumentNullException.ThrowIfNull(info);

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _mapper = new Mapper(this, info, factory);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public sealed class Mapper : IViewModelMapper
        {
            private readonly ScarletFile _viewModel;
            private readonly string _fullName;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public Mapper(ScarletFile viewModel, ScarletFileInfo info, IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fullName = info.FullName;

                Set(info);
            }

            private void Set(ScarletFileInfo info)
            {
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;
            }

            public async Task Refresh(CancellationToken token)
            {
                var info = await _fileSystemViewModelFactory.GetFileInfo(_fullName, token);
                if (info is null)
                {
                    return;
                }

                Set(info);
            }
        }
    }
}
