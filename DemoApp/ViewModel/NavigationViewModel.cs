using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.FileSystemBrowser;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(ICommandBuilder commandBuilder, LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder, localizationsViewModel)
        {
            Add("ParentViewModel", new ParentViewModel(commandBuilder));
            Add("Image loading", new ProcessingImagesViewModel(commandBuilder));
            Add("Drag and Drop", new ProcessingImagesViewModel(commandBuilder));
            Add("DataContextSchenanigansViewModel", new DataContextSchenanigansViewModel(commandBuilder));
            Add("AsyncCommands", new AsyncCommandViewModelStuff(commandBuilder));
            Add("Datagrid", new ParentsViewModel(commandBuilder));
            Add("Progress", new ProgressViewModel(commandBuilder));
            Add("Snake", new DummySnakeViewModel());
            Add("FileSystemBrowser", new FileSystemViewModel(commandBuilder, new FileSystemOptionsViewModel(commandBuilder)));
            Add("Busytree", new BusyViewModel(commandBuilder));
            Add("Text rendering", new TextDisplayViewModel(commandBuilder));
        }
    }
}
