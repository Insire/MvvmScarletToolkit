namespace MvvmScarletToolkit.Mediator
{
    /// <summary>
    /// Allows for generic type constraints of objects implementing IRequest or IRequest{TResponse}
    /// </summary>

    public interface ISimpleBaseRequest;

    /// <summary>
    /// Marker interface to represent a request with a void response
    /// </summary>
    public interface ISimpleRequest : ISimpleBaseRequest;

    /// <summary>
    /// Marker interface to represent a request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface ISimpleRequest<out TResponse> : ISimpleBaseRequest;

    /// <summary>
    /// Defines a handler for a request with a void response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0040:Add accessibility modifiers", Justification = "<Pending>")]
    public interface ISimpleRequestHandler<in TRequest>
        where TRequest : ISimpleRequest
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        Task Handle(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a handler for a request
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0040:Add accessibility modifiers", Justification = "<Pending>")]
    public interface ISimpleRequestHandler<in TRequest, TResponse>
        where TRequest : ISimpleRequest<TResponse>
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
