using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Samples
{
    public sealed class ContextMenuViewModel : ObservableObject
    {
        public string Name { get; } = "Test";
        public ObservableCollection<ContextMenuViewModel> Items { get; }

        public ContextMenuViewModel()
        {
            Items = new ObservableCollection<ContextMenuViewModel>();
        }
    }
}
