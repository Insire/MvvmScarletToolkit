using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public interface IBusyStack
    {
        BusyToken GetToken();

        Task Pull();

        Task Push(BusyToken token);
    }
}
