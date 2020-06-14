using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class FileSystemViewModel : BusinessViewModelListBase<IFileSystemDrive>
    {
        private readonly IFileSystemViewModelFactory _factory;

        [Bindable(true, BindingDirection.OneWay)]
        public FileSystemOptionsViewModel Options { get; }

        public FileSystemViewModel(IScarletCommandBuilder commandBuilder, IFileSystemViewModelFactory factory, FileSystemOptionsViewModel options)
            : base(commandBuilder)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var drives = await _factory.GetDrives(Options.DriveTypes, Options.FileAttributes, Options.FolderAttributes);

            await AddRange(drives);
        }
    }
}
