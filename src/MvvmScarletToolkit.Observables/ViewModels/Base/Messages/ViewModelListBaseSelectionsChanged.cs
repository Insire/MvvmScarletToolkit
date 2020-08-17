using System.Collections.Generic;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionsChanged<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanged(in object sender, in IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
