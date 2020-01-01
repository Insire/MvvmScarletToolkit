using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public class ViewModelListBaseSelectionChanged<TViewModel> : GenericScarletMessage<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        public ViewModelListBaseSelectionChanged(object sender, TViewModel content)
            : base(sender, content)
        {
        }
    }
}
