using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed class FolderPickerViewModel : FileSystemPickerViewModel
    {
        public FolderPickerViewModel(FileSystemViewModel fileSystemViewModel)
            : base(fileSystemViewModel, [])
        {
        }

        /// <summary>
        /// The folder selected in the detail view, otherwise the displayed folder or drive.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)] public override string? SelectedPath => (FileSystem.SelectedEntry as IFileSystemDirectory)?.FullName ?? FileSystem.SelectedContainer?.FullName;

        [Bindable(true, BindingDirection.OneWay)] public override string Title => "Select a folder";

        [Bindable(true, BindingDirection.OneWay)] public override string ConfirmText => "Select";
    }
}
