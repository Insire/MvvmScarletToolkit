using MvvmScarletToolkit.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        protected Scenes(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        protected Scenes(IEnumerable<Scene> content, IScarletDispatcher dispatcher)
            : this(dispatcher)
        {
            using (BusyStack.GetToken())
            {
                _ = base.AddRange(content).ContinueWith(_ => base.SelectedItem = Items.First((p) => p.IsSelected));
            }
        }
    }
}
