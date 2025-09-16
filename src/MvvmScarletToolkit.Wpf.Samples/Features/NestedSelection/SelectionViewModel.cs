using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class SelectionViewModel : ObservableObject
    {
        [ObservableProperty] private bool? _isSelected;
        [ObservableProperty] private int _index;

        public ObservableCollection<SelectionChildViewModel> Children { get; }

        [ObservableProperty] private SelectionChildViewModel _selectedChild;

        public SelectionViewModel()
        {
            Children = new ObservableCollection<SelectionChildViewModel>();

            Index = Random.Shared.Next(100, 1000);

            for (var i = 0; i < Random.Shared.Next(1, 20); i++)
            {
                Children.Add(new SelectionChildViewModel());
            }

            Children[0].IsSelected = true;
            SelectedChild = Children[0];
        }
    }
}
