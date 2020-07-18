using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusinessViewModelListBase<TViewModel> : IVirtualizationViewModel
        where TViewModel : class, INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<TViewModel> Items { get; }
    }
}
