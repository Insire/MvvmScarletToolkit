using System.Collections.Generic;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    public class ViewModelListBaseSelectionsChanged<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
        where TViewModel : class, INotifyPropertyChanged
    {
        public ViewModelListBaseSelectionsChanged(object sender, IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
