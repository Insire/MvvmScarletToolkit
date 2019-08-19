using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusinessViewModelListBase : IVirtualizationViewModel
    {
    }

    public interface IBusinessViewModelListBase<TViewModel> : IBusinessViewModelListBase
        where TViewModel : class, INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<TViewModel> Items { get; }
    }
}
