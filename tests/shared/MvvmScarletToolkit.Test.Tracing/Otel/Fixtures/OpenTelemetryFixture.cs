using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Fixtures
{
    public sealed class OpenTelemetryFixture : OpenTelemetryFixtureBase, IDisposable
    {
        private const string TracerName = "unit-test-with-otel";
        private readonly TracerProvider _tracerProvider;

        public OpenTelemetryFixture()
        {
            _tracerProvider = CreateTracerProvider(ResourceBuilder.CreateDefault().AddService(TracerName), TracerName);
        }

        public void Dispose()
        {
            _tracerProvider?.Dispose();
        }
    }

    public abstract class OpenTelemetryFixtureBase()
    {
        protected static TracerProvider CreateTracerProvider(ResourceBuilder builder, string tracerName)
        {
            return Sdk.CreateTracerProviderBuilder()
                 .SetResourceBuilder(builder)
                 .AddSource(tracerName)
                 .AddHttpClientInstrumentation()
                 .AddConsoleExporter()
                     .AddOtlpExporter(o =>
                     {
                         o.Endpoint = new Uri("http://localhost:4317");
                         o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                     })
                 .Build();
        }
    }
}
