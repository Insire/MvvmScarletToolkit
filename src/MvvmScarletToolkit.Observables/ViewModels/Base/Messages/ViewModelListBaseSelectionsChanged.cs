using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionsChanged<TViewModel> : ValueChangedMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanged(in IEnumerable<TViewModel> content)
            : base(content)
        {
        }
    }
}
