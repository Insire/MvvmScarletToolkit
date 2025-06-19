using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using MvvmScarletToolkit.Observables;
using System.Threading;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    public sealed class NavigationViewModel:Scenes
    {
        public NavigationViewModel(in IScarletCommandBuilder commandBuilder, in LocalizationsViewModel localizationsViewModel, SynchronizationContext synchronizationContext)
            : base(in commandBuilder, in localizationsViewModel)
        {
            Add("State changes in a tree structure", new BusyViewModel(commandBuilder));
            Add("Binding Enum values", new EnumViewModel());
            Add("Lazy Loading / Data-Virtualization", new DataEntriesViewModel(CommandBuilder, synchronizationContext));
            Add("ConcurrentCommands and state changes", new AsyncStateListViewModel(commandBuilder));
            Add("Progress, -notification and dispatcher throtteling", new ProgressViewModel(commandBuilder));
            Add("Binding Passwordbox", new PasswordViewModel());
            Add("MVVM Terminal/Console", new ProcessViewModel(commandBuilder));

            var contextMenu = new ContextMenuViewModels();
            var menuitem = new ContextMenuViewModel();
            menuitem.Items.Add(new ContextMenuViewModel());
            menuitem.Items.Add(new ContextMenuViewModel());

            contextMenu.Items[0].Items.Add(menuitem);

            Add("MVVM ContextMenus", contextMenu);
            Add("Input Prevention", new FormViewModel());
            Add("ObservableDictionary", new ObservableDictionaryViewModel());

            Items[0].IsSelected = true;
            SelectedItem = Items[0];
        }
    }
}
