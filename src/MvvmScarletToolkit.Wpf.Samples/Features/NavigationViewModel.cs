using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.FileSystemBrowser;
using System.Threading;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(SynchronizationContext synchronizationContext, IScarletCommandBuilder commandBuilder, LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder, localizationsViewModel)
        {
            var dataGridViewModel = new DataGridViewModel(commandBuilder, synchronizationContext);

            Add("Lazy Loading / Data-Virtualization", new DataEntriesViewModel(CommandBuilder, synchronizationContext));
            Add("Image Loading + Drag and Drop", new ProcessingImagesViewModel(commandBuilder, new ImageFactory(CommandBuilder)));
            Add("ConcurrentCommands and state changes", new AsyncStateListViewModel(commandBuilder));
            Add("MVVM Live Sorting and Grouping in bound collections", dataGridViewModel);
            Add("Progress, -notification and dispatcher throtteling", new ProgressViewModel(commandBuilder));
            Add("FileSystemBrowser", new FileSystemViewModel(commandBuilder, new FileSystemViewModelFactory(commandBuilder), FileSystemOptionsViewModel.Default));
            Add("State changes in a tree structure", new BusyViewModel(commandBuilder));
            Add("Geometry rendering", new GeometryRenderViewModel(commandBuilder));
            Add("Binding Passwordbox", new PasswordViewModel());
            Add("MVVM Grouping", GroupingViewModel.Create(commandBuilder, dataGridViewModel.Items));
            Add("Dialog-ViewModel", new DialogViewModel(commandBuilder));
            Add("MVVM Terminal/Console", new ProcessViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Binding Enum values", new EnumViewModel());
            Add("MVVM Toast-Notification", new ToastsViewModel(commandBuilder));
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());
        }
    }
}
