using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features
{
    public sealed partial class ObservableDictionaryViewModel : ObservableObject
    {
        public ObservableDictionary<int, Guid> Items { get; }

        public ObservableDictionaryViewModel()
        {
            Items = new ObservableDictionary<int, Guid>()
            {
                [0] = Guid.NewGuid(),
                [1] = Guid.NewGuid(),
                [2] = Guid.NewGuid(),
                [3] = Guid.NewGuid(),
                [4] = Guid.NewGuid(),
            };
        }

        [RelayCommand]
        private void Add()
        {
            if (Items.Count > 0)
            {
                Items.Add(Items.MaxBy(p => p.Key).Key + 1, Guid.NewGuid());
            }
            else
            {
                Items.Add(0, Guid.NewGuid());
            }
        }

        [RelayCommand(CanExecute = nameof(CanRemove))]
        private void Remove()
        {
            Items.Remove(Items.First());
        }

        private bool CanRemove()
        {
            return Items.Count > 0;
        }

        [RelayCommand]
        private void Clear()
        {
            Items.Clear();
        }
    }
}
