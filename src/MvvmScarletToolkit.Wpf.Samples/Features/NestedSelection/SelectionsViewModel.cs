using CommunityToolkit.Mvvm.ComponentModel;
using System;
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

            for (var i = 0; i < Random.Shared.Next(1, 20); i++)
            {
                Children.Add(new SelectionViewModel());
            }

            Children[0].IsSelected = true;
            SelectedChild = Children[0];
        }
    }
}
