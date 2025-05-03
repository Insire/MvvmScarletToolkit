using MediatR;

namespace MvvmScarletToolkit.Mediator.Benchmark.Util
{
    public readonly record struct RequestStructWithResponse(bool Value) : ISimpleRequest<ResponseStruct>, IRequest<ResponseStruct>;

    public readonly record struct ResponseStruct(bool Value);

    public sealed class RequestStructWithResponseHandler : ISimpleRequestHandler<RequestStructWithResponse, ResponseStruct>, IRequestHandler<RequestStructWithResponse, ResponseStruct>
    {
        public async Task<ResponseStruct> Handle(RequestStructWithResponse request, CancellationToken cancellationToken = default)
        {
            await ValueTask.CompletedTask;

            return new ResponseStruct(request.Value);
        }
    }
}
