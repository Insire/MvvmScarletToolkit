using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        protected Scenes(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }
    }
}
