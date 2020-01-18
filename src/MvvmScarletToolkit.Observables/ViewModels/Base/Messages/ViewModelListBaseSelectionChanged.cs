using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public class ViewModelListBaseSelectionChanged<TViewModel> : GenericScarletMessage<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
#pragma warning disable CS8604 // Possible null reference argument.

        public ViewModelListBaseSelectionChanged(object sender, TViewModel? content)
            : base(sender, content)
#pragma warning restore CS8604 // Possible null reference argument.
        {
        }
    }
}
