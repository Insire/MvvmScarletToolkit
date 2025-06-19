using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Core.Samples.Features.ContextMenu
{
    public sealed partial class ContextMenuViewModels : ObservableObject
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
