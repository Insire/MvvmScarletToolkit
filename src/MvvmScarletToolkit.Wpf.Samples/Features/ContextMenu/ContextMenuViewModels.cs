using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class ContextMenuViewModels
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
