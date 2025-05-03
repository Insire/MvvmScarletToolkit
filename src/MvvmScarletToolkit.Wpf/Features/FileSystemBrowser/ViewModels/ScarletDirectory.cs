using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("Directory: {Name} IsContainer: {IsContainer}")]
    public sealed partial class ScarletDirectory : BusinessViewModelListBase<IFileSystemChild>, IFileSystemDirectory
    {
        private readonly Mapper _mapper;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? Parent { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; } = string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; }= string.Empty;
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool Exists { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsHidden { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? CreationTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastAccessTimeUtc { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DateTime? LastWriteTimeUtc { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => true;

        public ScarletDirectory(
            ScarletDirectoryInfo info,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            IFileSystemParent parent,
            IScarletCommandBuilder commandBuilder,
            IFileSystemViewModelFactory factory)
            : base(commandBuilder)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(parent);

            Parent = parent;

            _mapper = new Mapper(this, info,fileAttributes,folderAttributes, factory);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }


        public sealed class Mapper : IViewModelMapper
        {
            private readonly ScarletDirectory _viewModel;
            private readonly IReadOnlyCollection<FileAttributes> _fileAttributes;
            private readonly IReadOnlyCollection<FileAttributes> _folderAttributes;
            private readonly string _fullName;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public Mapper(
                ScarletDirectory viewModel,
                ScarletDirectoryInfo info,
                IReadOnlyCollection<FileAttributes> fileAttributes,
                IReadOnlyCollection<FileAttributes> folderAttributes,
                IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(info);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileAttributes = fileAttributes;
                _folderAttributes = folderAttributes;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fullName = info.FullName;
                Set(info);
            }

            private void Set(ScarletDirectoryInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;
            }

            public async Task Refresh(CancellationToken token)
            {
                var info = await _fileSystemViewModelFactory.GetDirectoryInfo(_fullName, token);
                if (info is null)
                {
                    return;
                }

                Set(info);

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel);
                if (isEmpty)
                {
                    await _viewModel.Clear(token);
                    return;
                }

                var children = await _fileSystemViewModelFactory.GetChildren(_viewModel, _fileAttributes, _folderAttributes);

                if (!_viewModel.IsLoaded)
                {
                    await _viewModel.AddRange(children, token);
                }
                else
                {
                    await _viewModel.Dispatcher.Invoke(() => _viewModel.Items.UpdateItems(children, FileSystemViewModelFactoryExtensions.IFileSystemChildComparer, FileSystemViewModelFactoryExtensions.IFileSystemChildMapper));
                }
            }
        }
    }
}
