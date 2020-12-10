using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

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
