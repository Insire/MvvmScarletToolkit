namespace MvvmScarletToolkit.Mediator
{
    public interface ISimplePublisher
    {
        Task Publish(ISimpleNotification notification, CancellationToken cancellationToken = default);
    }
}