using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MvvmScarletToolkit.Mediator.Tests.Otel
{
    public sealed class OpenTelemetryMonitoredFixture : IDisposable
    {
        public static ActivitySource ActivitySource { get; } = new ActivitySource(TracerName);

        private const string TracerName = "unittest-with-otel";
        private readonly TracerProvider? _tracerProvider;

        public OpenTelemetryMonitoredFixture()
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MvvmScarletToolkit"))
                .AddProcessor(new TestRunSpanProcessor(Guid.NewGuid().ToString()))
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
            BaseTraceTestAttribute.ActivityForTestRun?.Stop();
            _tracerProvider?.ForceFlush();
            _tracerProvider?.Dispose();
        }
    }
}
