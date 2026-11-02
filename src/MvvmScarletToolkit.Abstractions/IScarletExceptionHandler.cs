namespace MvvmScarletToolkit
{
    public interface IScarletExceptionHandler
    {
        Task Handle(Exception ex);
    }
}
