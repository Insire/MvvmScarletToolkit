using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MvvmScarletToolkit.Mediator.Tests.Otel
{
    public sealed class OpenTelemetryFixture : IDisposable
    {
        private const string TracerName = "unit-test-with-otel";
        private readonly TracerProvider _tracerProvider;

        public OpenTelemetryFixture()
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TracerName))
                .AddSource(TracerName)
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://localhost:4317");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    })
                .Build();
        }

        public void Dispose()
        {
            _tracerProvider?.Dispose();
        }
    }
}
