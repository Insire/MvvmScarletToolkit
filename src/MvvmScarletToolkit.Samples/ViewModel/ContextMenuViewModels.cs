using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Samples
{
    public sealed class ContextMenuViewModels : ObservableObject
    {
        public ObservableCollection<ContextMenuViewModel> Items { get; }

        public ContextMenuViewModels()
        {
            Items = new ObservableCollection<ContextMenuViewModel>()
            {
                new ContextMenuViewModel(),
            };
        }
    }
}
