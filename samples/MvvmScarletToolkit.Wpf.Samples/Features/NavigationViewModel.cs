using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Core.Samples.Features.Enums;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using MvvmScarletToolkit.Wpf.Samples.Features.Geometry;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using System.Net.Http;
using System.Reactive.Concurrency;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(
            IScheduler scheduler,
            IFileSystemViewModelFactory factory,
            SynchronizationContext synchronizationContext,
            IScarletCommandBuilder commandBuilder,
            LocalizationsViewModel localizationsViewModel,
            ProcessingImagesViewModel processingImagesViewModel,
            HttpClient httpClient)
            : base(commandBuilder, localizationsViewModel)
        {
            var dataGridViewModel = new DataGrid.DataGridViewModel(commandBuilder, synchronizationContext);

            Add("Image Loading + Drag and Drop", processingImagesViewModel);
            Add("Lazy Loading / Data-Virtualization", new DataEntriesViewModel(CommandBuilder, synchronizationContext));
            Add("Image Loading in a virtualized list", new VirtualizedImagesViewModel(CommandBuilder, synchronizationContext, httpClient));
            Add("ConcurrentCommands and state changes", new AsyncStateListViewModel(commandBuilder));
            Add("MVVM Live Sorting and Grouping in bound collections", dataGridViewModel);
            Add("Progress, -notification and dispatcher throtteling", new ProgressViewModel(commandBuilder));
            Add("FileSystemBrowser", new FileSystemViewModel(scheduler, factory, FileSystemOptionsViewModel.Default));
            Add("State changes in a tree structure", new BusyViewModel(commandBuilder));
            Add("Geometry rendering", new GeometryRenderViewModel(commandBuilder));
            Add("Binding Passwordbox", new PasswordViewModel());
            // the same instance as in the DataGrid scene: GroupsViewModels coordinate through the shared messenger,
            // a second GroupingViewModel would take part in that and steal groupings from the first one
            Add("MVVM Grouping", dataGridViewModel.Groups);
            Add("Dialog-ViewModel", new DialogViewModel(commandBuilder));
            Add("MVVM Terminal/Console", new ProcessViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Binding Enum values", new EnumsViewModel(commandBuilder));
            Add("MVVM Toast-Notification", new ToastsViewModel(commandBuilder));
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());
            Add("Nested Selection", new SelectionsViewModel());

            Items[0].IsSelected = true;
        }
    }
}
