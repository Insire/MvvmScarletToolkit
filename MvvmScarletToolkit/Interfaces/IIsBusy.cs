namespace MvvmScarletToolkit
{
    public interface IIsBusy
    {
        BusyStack BusyStack { get; }
        bool IsBusy { get; }
    }
}
