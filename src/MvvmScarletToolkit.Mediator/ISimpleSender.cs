namespace MvvmScarletToolkit.Mediator
{
    public interface ISimpleSender
    {
        Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest;

        Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest<TResponse>;
    }
}
