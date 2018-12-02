namespace MvvmScarletToolkit.Observables
{
    public interface IBusyStack
    {
        BusyToken GetToken();

        bool Pull();

        void Push(BusyToken token);
    }
}
