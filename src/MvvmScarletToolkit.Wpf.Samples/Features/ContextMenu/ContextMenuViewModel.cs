using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class ContextMenuViewModel
    {
        public string Name { get; } = "Test";
        public ObservableCollection<ContextMenuViewModel> Items { get; }

        public ContextMenuViewModel()
        {
            Items = new ObservableCollection<ContextMenuViewModel>();
        }
    }
}
