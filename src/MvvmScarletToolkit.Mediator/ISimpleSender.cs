namespace MvvmScarletToolkit.Mediator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0040:Add accessibility modifiers", Justification = "<Pending>")]
    public interface ISimpleSender
    {
        Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest;

        Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest<TResponse>;
    }
}
