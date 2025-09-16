using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed partial class SelectionViewModel : ObservableObject
    {
        [ObservableProperty] private bool? _isSelected;
        [ObservableProperty] private SelectionChildViewModel _selectedChild;

        public int Index { get; }
        public ObservableCollection<SelectionChildViewModel> Children { get; }

        public SelectionViewModel(int index)
        {
            Children = new ObservableCollection<SelectionChildViewModel>();
            Index = index;

            for (var i = 0; i < 20; i++)
            {
                Children.Add(new SelectionChildViewModel(i + (1000 * Index)));
            }

            Children[0].IsSelected = true;
            SelectedChild = Children[0];
        }

        private string GetDebuggerDisplay()
        {
            return Index.ToString();
        }
    }
}
