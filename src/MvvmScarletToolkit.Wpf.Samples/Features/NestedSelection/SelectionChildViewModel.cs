using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class SelectionChildViewModel : ObservableObject
    {
        [ObservableProperty] private bool? _isSelected;
        [ObservableProperty] private int _index;

        public SelectionChildViewModel()
        {
            Index = Random.Shared.Next(1, 100);
        }
    }
}
