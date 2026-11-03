using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.ComponentModel;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Selects where a file gets saved: the folder displayed in the browser and an editable <see cref="FileName"/>.
    /// <list type="bullet">
    /// <item>Selecting a file in the browser takes over its name, opening it confirms.</item>
    /// <item>A name without an extension gets the one of the selected filter, e.g. <c>.html</c> for <c>*.html</c>.</item>
    /// <item><see cref="ConfirmOverwrite"/> is asked before an existing file is replaced.</item>
    /// </list>
    /// </summary>
    public sealed partial class SaveFilePickerViewModel : FileSystemPickerViewModel
    {
        public SaveFilePickerViewModel(FileSystemViewModel fileSystemViewModel, string fileName, params FileSystemFilter[] filters)
            : base(fileSystemViewModel, filters)
        {
            FileName = fileName ?? string.Empty;
        }

        /// <summary>
        /// The name of the file, without a folder.
        /// </summary>
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial string FileName { get; set; }

        /// <summary>
        /// Asked with the full path before an existing file is replaced, the dialog stays open when it returns <see langword="false"/>.
        /// Without it, existing files are replaced.
        /// </summary>
        public Func<string, bool>? ConfirmOverwrite { get; set; }

        /// <summary>
        /// The displayed folder combined with <see cref="FileName"/>, <see langword="null"/> while the name is empty or invalid.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public override string? SelectedPath
        {
            get
            {
                var folder = FileSystem.SelectedContainer?.FullName;
                var fileName = FileName?.Trim();

                if (folder is null || string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return null;
                }

                return Path.Combine(folder, WithDefaultExtension(fileName, SelectedFilter));
            }
        }

        [Bindable(true, BindingDirection.OneWay)] public override string Title => "Save file";

        [Bindable(true, BindingDirection.OneWay)] public override string ConfirmText => "Save";

        partial void OnFileNameChanged(string value)
        {
            OnSelectedPathChanged();
        }

        protected override void OnSelectionChanged(string propertyName)
        {
            if (propertyName == nameof(FileSystemViewModel.SelectedFile) && FileSystem.SelectedFile is { } file)
            {
                FileName = file.Name;
            }
        }

        protected override void OnFileOpened(object? sender, IFileSystemChild file)
        {
            FileName = file.Name;
            ConfirmCommand.Execute(null);
        }

        protected override bool CanComplete()
        {
            return SelectedPath is not { } path
                || !File.Exists(path)
                || ConfirmOverwrite?.Invoke(path) != false;
        }

        /// <summary>
        /// Appends the extension of a filter like <c>*.html</c>, when <paramref name="fileName"/> has none.
        /// </summary>
        internal static string WithDefaultExtension(string fileName, FileSystemFilter? filter)
        {
            if (Path.HasExtension(fileName) || filter is null)
            {
                return fileName;
            }

            var pattern = filter.Patterns[0];
            if (!pattern.StartsWith("*.", StringComparison.Ordinal) || pattern.AsSpan(2).IndexOfAny('*', '?') >= 0)
            {
                return fileName;
            }

            return fileName + pattern[1..];
        }
    }
}
