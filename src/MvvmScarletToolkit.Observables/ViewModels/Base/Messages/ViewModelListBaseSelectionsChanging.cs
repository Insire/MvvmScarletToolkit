using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionsChanging<TViewModel> : ValueChangedMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanging(in IEnumerable<TViewModel> content)
            : base(content)
        {
        }
    }
}
