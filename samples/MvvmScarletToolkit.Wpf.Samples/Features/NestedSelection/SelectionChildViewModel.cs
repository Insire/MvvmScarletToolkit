using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed partial class SelectionChildViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool? IsSelected { get; set; }
        public int Index { get; }

        public SelectionChildViewModel(int index)
        {
            Index = index;
        }

        private string GetDebuggerDisplay()
        {
            return Index.ToString();
        }
    }
}
