using BenchmarkDotNet.Attributes;
using MvvmScarletToolkit.Mediator.Benchmark.Util;

namespace MvvmScarletToolkit.Mediator.Benchmark
{
    public class RequestBenchmark
    {
        private readonly SimpleMediatorRequests _simpleMediator;
        private readonly MediatRRequests _mediatr;

        public RequestBenchmark()
        {
            _simpleMediator = new SimpleMediatorRequests();
            _mediatr = new MediatRRequests();
        }

        [Benchmark(Baseline = true)]
        public Task MediatR_Send_Request_With_Response()
            => _mediatr.Send_Request_With_Response();

        [Benchmark()]
        public Task MediatR_Send_Request()
            => _mediatr.Send_Request();

        [Benchmark]
        public Task SimpleMediator_Send_Request_With_Response()
            => _simpleMediator.Send_Request_With_Response();

        [Benchmark]
        public Task SimpleMediator_Send_Request()
            => _simpleMediator.Send_Request();
    }
}
