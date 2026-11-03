using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Selects a path from a <see cref="FileSystemViewModel"/>, which stays owned by the caller.
    /// </summary>
    public abstract partial class FileSystemPickerViewModel : ObservableObject, IDisposable
    {
        protected FileSystemPickerViewModel(FileSystemViewModel fileSystemViewModel, IReadOnlyList<FileSystemFilter> filters)
        {
            ArgumentNullException.ThrowIfNull(fileSystemViewModel);
            ArgumentNullException.ThrowIfNull(filters);

            FileSystem = fileSystemViewModel;
            Filters = filters;
            SelectedFilter = filters.FirstOrDefault();

            FileSystem.PropertyChanged += OnFileSystemChanged;
            FileSystem.FileOpened += OnFileOpened;
        }

        [Bindable(true, BindingDirection.OneWay)] public FileSystemViewModel FileSystem { get; }

        /// <summary>
        /// The filters to choose from, the first one is applied initially.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)] public IReadOnlyList<FileSystemFilter> Filters { get; }

        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial FileSystemFilter? SelectedFilter { get; set; }

        /// <summary>
        /// The path that gets confirmed, <see langword="null"/> while nothing suitable is selected.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)] public abstract string? SelectedPath { get; }

        [Bindable(true, BindingDirection.OneWay)] public abstract string Title { get; }

        [Bindable(true, BindingDirection.OneWay)] public abstract string ConfirmText { get; }

        /// <summary>
        /// Raised by <see cref="ConfirmCommand"/>, the <see cref="SelectedPath"/> is the result.
        /// </summary>
        public event EventHandler? Confirmed;

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private void Confirm()
        {
            if (!CanComplete())
            {
                return;
            }

            Confirmed?.Invoke(this, EventArgs.Empty);
        }

        private bool CanConfirm()
        {
            return SelectedPath is not null;
        }

        /// <summary>
        /// Last check before <see cref="Confirmed"/> is raised, e.g. to ask before overwriting a file.
        /// </summary>
        protected virtual bool CanComplete()
        {
            return true;
        }

        /// <summary>
        /// Raises the change notification of <see cref="SelectedPath"/> and updates <see cref="ConfirmCommand"/>.
        /// </summary>
        protected void OnSelectedPathChanged()
        {
            OnPropertyChanged(nameof(SelectedPath));
            ConfirmCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Called when the selected file, entry or folder of <see cref="FileSystem"/> changed.
        /// </summary>
        protected virtual void OnSelectionChanged(string propertyName)
        {
        }

        partial void OnSelectedFilterChanged(FileSystemFilter? value)
        {
            FileSystem.FileFilter = value;

            // a save picker derives the extension from the filter
            OnSelectedPathChanged();
        }

        /// <summary>
        /// Double clicking a file ends a file selection, folders are opened instead.
        /// </summary>
        protected virtual void OnFileOpened(object? sender, IFileSystemChild file)
        {
        }

        private void OnFileSystemChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FileSystemViewModel.SelectedFile):
                case nameof(FileSystemViewModel.SelectedEntry):
                case nameof(FileSystemViewModel.SelectedContainer):
                    OnSelectionChanged(e.PropertyName);
                    OnSelectedPathChanged();
                    break;
            }
        }

        public void Dispose()
        {
            FileSystem.PropertyChanged -= OnFileSystemChanged;
            FileSystem.FileOpened -= OnFileOpened;
        }
    }
}
