using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    public class ViewModelListBaseSelectionChanging<TViewModel> : GenericScarletMessage<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        public ViewModelListBaseSelectionChanging(object sender, TViewModel content)
            : base(sender, content)
        {
        }
    }
}
