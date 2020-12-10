using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanging<TViewModel> : ValueChangedMessage<TViewModel>
    {
        public object Sender { get; }

        public ViewModelListBaseSelectionChanging(in object sender, in TViewModel content)
            : base(content)
        {
            Sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }
    }
}
