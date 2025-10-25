using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class SelectionsViewModel : ObservableObject
    {
        public ObservableCollection<SelectionViewModel> Children { get; }

        [ObservableProperty] private SelectionViewModel _selectedChild;

        public SelectionsViewModel()
        {
            Children = new ObservableCollection<SelectionViewModel>();

            for (var i = 1; i < 20; i++)
            {
                Children.Add(new SelectionViewModel(i));
            }

            Children[0].IsSelected = true;
            SelectedChild = Children[0];
        }
    }
}
