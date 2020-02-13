using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    public sealed class ViewModelListBaseSelectionsChanged<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanged(object sender, IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
