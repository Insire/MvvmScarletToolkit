using MvvmScarletToolkit;
using MvvmScarletToolkit.FileSystemBrowser;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(ICommandBuilder commandBuilder, LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder, localizationsViewModel)
        {
            Add(nameof(VirtualizationViewModel), new VirtualizationViewModel(new LogItems(CommandBuilder, new DemoCache()), commandBuilder)); // TODO move item creationlogic to a service
            Add("Image Drag and Drop", new ProcessingImagesViewModel(commandBuilder));
            Add(nameof(AsyncStateListViewModel), new AsyncStateListViewModel(commandBuilder));
            Add(nameof(DataGridViewModel), new DataGridViewModel(commandBuilder));
            Add(nameof(ProgressViewModel), new ProgressViewModel(commandBuilder));
            Add("Snake", new DummySnakeViewModel());
            Add("FileSystemBrowser", new FileSystemViewModel(commandBuilder, new FileSystemOptionsViewModel(commandBuilder)));
            Add("Busytree", new BusyViewModel(commandBuilder));
            Add("Text rendering", new TextDisplayViewModel(commandBuilder));
            Add(nameof(PasswordViewModel), new PasswordViewModel());
            Add(nameof(GroupingViewModel), new GroupingViewModel(commandBuilder, typeof(DataGridRowViewModel)));
            Add(nameof(DialogViewModel), new DialogViewModel(commandBuilder));
        }
    }
}
