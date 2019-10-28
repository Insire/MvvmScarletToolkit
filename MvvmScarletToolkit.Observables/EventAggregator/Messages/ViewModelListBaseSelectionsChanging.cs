using System.Collections.Generic;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public class ViewModelListBaseSelectionsChanging<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
        where TViewModel : class, INotifyPropertyChanged
    {
        public ViewModelListBaseSelectionsChanging(object sender, IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
