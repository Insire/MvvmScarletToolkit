using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Core.Samples.Features.NestedSelection
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed partial class SelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool? IsSelected { get; set; }

        // Avalonia's ComboBox writes null back when its ItemsSource gets replaced, which would lose the selection of this instance
        public SelectionChildViewModel SelectedChild
        {
            get { return field; }
            set
            {
                if (value is not null)
                {
                    SetProperty(ref field, value);
                }
            }
        }

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
