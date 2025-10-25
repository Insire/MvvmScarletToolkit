using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Fixtures
{
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
