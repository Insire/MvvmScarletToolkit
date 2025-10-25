namespace MvvmScarletToolkit.Mediator
{
    public interface ISimpleNotification;

    public interface ISimpleNotificationHandler<in TNotification>
        where TNotification : ISimpleNotification
    {
        Task Handle(TNotification request, CancellationToken cancellationToken = default);
    }
}
