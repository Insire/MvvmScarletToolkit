using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        protected Scenes(IEnumerable<Scene> content)
        {
            using (BusyStack.GetToken())
            {
                AddRange(content);

                SelectedItem = Items.First((p) => p.IsSelected);
            }
        }
    }
}
