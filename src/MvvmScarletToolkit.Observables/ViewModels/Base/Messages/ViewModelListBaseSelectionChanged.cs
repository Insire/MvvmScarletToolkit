using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanged<TViewModel> : ValueChangedMessage<TViewModel>
    {
        public object Sender { get; }

        public ViewModelListBaseSelectionChanged(in object sender, in TViewModel content)
            : base(content)
        {
            Sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }
    }
}
