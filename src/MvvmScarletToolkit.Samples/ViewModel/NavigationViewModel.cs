using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.FileSystemBrowser;
using System.Windows.Data;

namespace MvvmScarletToolkit.Samples
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(IScarletCommandBuilder commandBuilder, LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder, localizationsViewModel)
        {
            var dataGridViewModel = new DataGridViewModel(commandBuilder);

            Add(nameof(VirtualizationViewModel), new VirtualizationViewModel(new LogItems(CommandBuilder), commandBuilder)); // TODO move item creationlogic to a service
            Add("Image Drag and Drop", new ProcessingImagesViewModel(commandBuilder, new ImageFactory(CommandBuilder)));
            Add(nameof(AsyncStateListViewModel), new AsyncStateListViewModel(commandBuilder));
            Add(nameof(DataGridViewModel), dataGridViewModel);
            Add(nameof(ProgressViewModel), new ProgressViewModel(commandBuilder));
            Add("Snake", new DummySnakeViewModel());
            Add("FileSystemBrowser", new FileSystemViewModel(commandBuilder, new FileSystemViewModelFactory(commandBuilder), FileSystemOptionsViewModel.Default));
            Add("Busytree", new BusyViewModel(commandBuilder));
            Add("Text rendering", new TextDisplayViewModel(commandBuilder));
            Add(nameof(PasswordViewModel), new PasswordViewModel());
            Add(nameof(GroupingViewModel), new GroupingViewModel(commandBuilder, () => CollectionViewSource.GetDefaultView(dataGridViewModel.Items), typeof(DataGridRowViewModel)));
            Add(nameof(DialogViewModel), new DialogViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add(nameof(ContextMenuViewModels), contextMenu);
            Add(nameof(EnumViewModel), new EnumViewModel());
        }
    }
}
