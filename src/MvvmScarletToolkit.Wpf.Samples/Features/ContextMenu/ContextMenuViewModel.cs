using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features.ContextMenu
{
    public sealed partial class ContextMenuViewModel : ObservableObject
    {
        public string Name { get; } = "Test";
        public ObservableCollection<ContextMenuViewModel> Items { get; }

        public ContextMenuViewModel()
        {
            Items = new ObservableCollection<ContextMenuViewModel>();
        }
    }
}
