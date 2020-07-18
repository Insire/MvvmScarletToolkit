using System.Collections.Generic;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionsChanging<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanging(in object sender, in IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
