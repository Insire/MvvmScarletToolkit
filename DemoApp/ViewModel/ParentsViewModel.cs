using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class ParentsViewModel : ViewModelListBase<ParentViewModel>
    {
        public ParentsViewModel()
            : base(new[]
            {
                new ParentViewModel(),
                    new ParentViewModel(),
                    new ParentViewModel(),
                    new ParentViewModel(),
            })
        {
            SelectedItem = this[0];
        }
    }
}
