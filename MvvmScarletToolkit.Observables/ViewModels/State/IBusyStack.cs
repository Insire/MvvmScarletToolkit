namespace MvvmScarletToolkit.Observables
{
    public interface IBusyStack
    {
        BusyToken GetToken();

        void Pull();

        void Push(BusyToken token);
    }
}
