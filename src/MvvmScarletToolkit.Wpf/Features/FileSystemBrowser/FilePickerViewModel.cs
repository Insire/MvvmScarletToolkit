using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed class FilePickerViewModel : FileSystemPickerViewModel
    {
        public FilePickerViewModel(FileSystemViewModel fileSystemViewModel, params FileSystemFilter[] filters)
            : base(fileSystemViewModel, filters)
        {
        }

        [Bindable(true, BindingDirection.OneWay)] public override string? SelectedPath => FileSystem.SelectedFile?.FullName;

        [Bindable(true, BindingDirection.OneWay)] public override string Title => "Select a file";

        [Bindable(true, BindingDirection.OneWay)] public override string ConfirmText => "Open";

        protected override void OnFileOpened(object? sender, IFileSystemChild file)
        {
            ConfirmCommand.Execute(null);
        }
    }
}
