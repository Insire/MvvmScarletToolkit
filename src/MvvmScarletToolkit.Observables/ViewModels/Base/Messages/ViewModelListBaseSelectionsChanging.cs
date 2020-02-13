using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    public sealed class ViewModelListBaseSelectionsChanging<TViewModel> : GenericScarletMessage<IEnumerable<TViewModel>>
    {
        public ViewModelListBaseSelectionsChanging(object sender, IEnumerable<TViewModel> content)
            : base(sender, content)
        {
        }
    }
}
