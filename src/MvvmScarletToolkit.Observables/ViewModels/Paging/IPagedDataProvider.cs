using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public interface IPagedDataProvider<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        Task<ICollection<TViewModel>> Get(int index, int count, CancellationToken token);
    }
}
