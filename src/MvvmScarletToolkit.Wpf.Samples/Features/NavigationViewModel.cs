using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Samples.Features.DataGrid;
using MvvmScarletToolkit.Wpf.Samples.Features.Geometry;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(
            DataGridViewModel dataGridViewModel,
            ProcessingImagesViewModel processingImagesViewModel,
            DataEntriesViewModel dataEntriesViewModel,
            AsyncStateListViewModel asyncStateListViewModel,
            ProgressViewModel progressViewModel,
            FileSystemViewModel fileSystemViewModel,
            FilePickerViewModel filePickerViewModel,
            FolderPickerViewModel folderPickerViewModel,
            BusyViewModel busyViewModel,
            GeometryRenderViewModel geometryRenderViewModel,
            PasswordViewModel passwordViewModel,
            DialogViewModel dialogViewModel,
            ProcessViewModel processViewModel,
            ToastsViewModel toastsViewModel,
            IScarletCommandBuilder commandBuilder,
            LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder, localizationsViewModel)
        {
            Add("Image Loading + Drag and Drop", processingImagesViewModel);
            Add("Lazy Loading / Data-Virtualization", dataEntriesViewModel);
            Add("ConcurrentCommands and state changes", asyncStateListViewModel);
            Add("MVVM Live Sorting and Grouping in bound collections", dataGridViewModel);
            Add("Progress, -notification and dispatcher throttling", progressViewModel);
            Add("File Browser", fileSystemViewModel);
            Add("File Picker", filePickerViewModel);
            Add("Folder Picker", folderPickerViewModel);
            Add("State changes in a tree structure", busyViewModel);
            Add("Geometry rendering", geometryRenderViewModel);
            Add("Binding PasswordBox", passwordViewModel);
            Add("MVVM Grouping", GroupingViewModel.Create(commandBuilder, dataGridViewModel.Items));
            Add("Dialog-ViewModel", dialogViewModel);
            Add("MVVM Terminal/Console", processViewModel);

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Binding Enum values", new EnumViewModel());
            Add("MVVM Toast-Notification", toastsViewModel);
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());

            Items[5].IsSelected = true;
        }
    }
}
