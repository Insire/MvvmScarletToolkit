using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public class ViewModelListBaseSelectionChanging<TViewModel> : GenericScarletMessage<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
#pragma warning disable CS8604 // Possible null reference argument.

        public ViewModelListBaseSelectionChanging(object sender, TViewModel? content)
            : base(sender, content)
#pragma warning restore CS8604 // Possible null reference argument.
        {
        }
    }
}
