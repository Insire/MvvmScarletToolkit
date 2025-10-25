using MediatR;

namespace MvvmScarletToolkit.Mediator.Benchmark.Util
{
    public readonly record struct RequestStruct(bool Value) : ISimpleRequest, IRequest;

    public sealed class RequestStructHandler : ISimpleRequestHandler<RequestStruct>, IRequestHandler<RequestStruct>
    {
        public async Task Handle(RequestStruct request, CancellationToken cancellationToken = default)
        {
            await ValueTask.CompletedTask;
        }
    }
}
