using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Samples.Features;
using MvvmScarletToolkit.Wpf.Samples.Features.AsyncState;
using MvvmScarletToolkit.Wpf.Samples.Features.Busy;
using MvvmScarletToolkit.Wpf.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Wpf.Samples.Features.Process;
using MvvmScarletToolkit.Wpf.Samples.Features.Virtualization;
using System.Threading;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    public sealed class NavigationViewModel:Scenes
    {
        public NavigationViewModel(in IScarletCommandBuilder commandBuilder, in LocalizationsViewModel localizationsViewModel, SynchronizationContext synchronizationContext)
            : base(in commandBuilder, in localizationsViewModel)
        {
            Add("Lazy Loading / Data-Virtualization", new DataEntriesViewModel(CommandBuilder, synchronizationContext));
            Add("ConcurrentCommands and state changes", new AsyncStateListViewModel(commandBuilder));
            Add("Progress, -notification and dispatcher throtteling", new ProgressViewModel(commandBuilder));
            Add("State changes in a tree structure", new BusyViewModel(commandBuilder));
            Add("Binding Passwordbox", new PasswordViewModel());
            Add("MVVM Terminal/Console", new ProcessViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Binding Enum values", new EnumViewModel());
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());

            Items[0].IsSelected = true;
        }
    }
}
