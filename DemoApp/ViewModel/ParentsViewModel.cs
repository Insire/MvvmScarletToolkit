using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class ParentsViewModel : ViewModelListBase<ParentViewModel>
    {
        public ParentsViewModel(IScarletDispatcher dispatcher)
            : base(new[]
            {
                new ParentViewModel(),
                    new ParentViewModel(),
                    new ParentViewModel(),
                    new ParentViewModel(),
            }, dispatcher)
        {
        }
    }
}
