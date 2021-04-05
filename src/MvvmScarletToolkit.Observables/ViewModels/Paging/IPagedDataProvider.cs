using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IPagedDataProvider<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        Task<ICollection<TViewModel>> Get(int index, int count, CancellationToken token);
    }
}
