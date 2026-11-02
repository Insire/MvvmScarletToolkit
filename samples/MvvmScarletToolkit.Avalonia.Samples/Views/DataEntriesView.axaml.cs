using Avalonia.Controls;
using MvvmScarletToolkit.Avalonia.Samples.Behaviors;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using System.Windows.Input;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class DataEntriesView : UserControl
    {
        private readonly ContainerLifecycle<DataEntryViewModel> _lifecycle;

        public DataEntriesView()
        {
            InitializeComponent();

            // entries load their data when they scroll into view and unload it again afterwards
            _lifecycle = new ContainerLifecycle<DataEntryViewModel>(Entries, item => Execute(item.LoadCommand), item => Execute(item.UnloadCommand));
        }

        private void Entries_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // containers are virtualized, so a binding on them could only ever see the selection of realized items
            foreach (var item in e.RemovedItems.OfType<DataEntryViewModel>())
            {
                item.IsSelected = false;
            }

            foreach (var item in e.AddedItems.OfType<DataEntryViewModel>())
            {
                item.IsSelected = true;
            }
        }

        private static void Execute(ICommand command)
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        }
    }
}
