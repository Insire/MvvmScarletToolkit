using MvvmScarletToolkit.Avalonia.Samples.Features.Geometry;
using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Core.Samples.Features.Dialog;
using MvvmScarletToolkit.Core.Samples.Features.Enums;
using MvvmScarletToolkit.Core.Samples.Features.Image;
using MvvmScarletToolkit.Core.Samples.Features.NestedSelection;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using MvvmScarletToolkit.Observables;
using System.Net.Http;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    // same scenes and order as the WPF sample. The DataGrid, grouping and FileSystemBrowser scenes are missing, those features only exist in the WPF package
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(
            IScarletCommandBuilder commandBuilder,
            LocalizationsViewModel localizationsViewModel,
            SynchronizationContext synchronizationContext,
            ProcessingImagesViewModel processingImagesViewModel,
            HttpClient httpClient)
            : base(in commandBuilder, in localizationsViewModel)
        {
            Add("Image Loading + Drag and Drop", processingImagesViewModel);
            Add("Lazy Loading / Data-Virtualization", new DataEntriesViewModel(CommandBuilder, synchronizationContext));
            Add("Image Loading in a virtualized list", new VirtualizedImagesViewModel(CommandBuilder, synchronizationContext, httpClient));
            Add("ConcurrentCommands and state changes", new AsyncStateListViewModel(commandBuilder));
            Add("Progress, -notification and dispatcher throtteling", new ProgressViewModel(commandBuilder));
            Add("State changes in a tree structure", new BusyViewModel(commandBuilder));
            Add("Geometry rendering", new GeometryRenderViewModel(commandBuilder));
            Add("Binding Passwordbox", new PasswordViewModel());
            Add("Dialog-ViewModel", new DialogViewModel(commandBuilder));
            Add("MVVM Terminal/Console", new ProcessViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Binding Enum values", new EnumsViewModel(commandBuilder));
            Add("MVVM Toast-Notification", new ToastsViewModel(new DesktopToastManager()));
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());
            Add("Nested Selection", new SelectionsViewModel());

            Items[0].IsSelected = true;
            SelectedItem = Items[0];
        }
    }
}
